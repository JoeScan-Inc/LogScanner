using Autofac;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Helpers;
using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.Collections.ObjectModel;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Models
{
    public class LogScannerEngine : IDisposable
    {
        #region Private Fields

        private const int maxUserMessages = 500;
        private readonly ILogArchiver archiver;
        private readonly RawProfileDumper dumper;
        private readonly List<IHeartBeatSubscriber> heartBeatSubscribers;
        private readonly CancellationTokenSource statusCheckerSource = new CancellationTokenSource();

        private readonly IEnumerable<IScannerAdapter> availableAdapters;
        private IDisposable? unlinker;
        private ObservableCollection<UserMessage> pluginMessages = new();

        #endregion

        public IFlightsAndWindowFilter Filter { get; }
        public CoreConfig Config { get; }
        public IReadOnlyList<IScannerAdapter> AvailableAdapters => new List<IScannerAdapter>(availableAdapters);
        public IScannerAdapter? ActiveAdapter { get; private set; }
        private ILogger Logger { get; }
        public ILogAssembler LogAssembler { get; }
        public LogModelBuilder ModelBuilder { get; }
        public IEnumerable<ILogModelConsumerPlugin> Consumers { get; }

        public ObservableCollection<UserMessage> PluginMessages => pluginMessages;

        public BroadcastBlock<Profile> RawProfilesBroadcastBlock { get; private set; }
            = new BroadcastBlock<Profile>(profile => profile);

        public BroadcastBlock<RawLog> RawLogsBroadcastBlock { get; }
            = new BroadcastBlock<RawLog>(r => r);

        public BroadcastBlock<LogModelResult> LogModelBroadcastBlock { get; }
            = new BroadcastBlock<LogModelResult>(r => r);

        public bool IsRunning => ActiveAdapter is { IsRunning: true };


        #region Event Handlers

        public event EventHandler ScanningStarted;
        public event EventHandler ScanningStopped;
        public event EventHandler ScanErrorEncountered;
        public event EventHandler<EncoderUpdateArgs> EncoderUpdated;
        public event EventHandler<PluginMessageEventArgs> PluginMessageReceived;
        public event EventHandler AdapterChanged;

        #endregion

        #region Event Invocation

        protected virtual void OnAdapterChanged()
        {
            AdapterChanged?.Raise(this, EventArgs.Empty);
        }

        private void ActiveAdapterOnScanningStarted(object? sender, EventArgs e)
        {
            ScanningStarted?.Raise(this, e);
        }

        private void ActiveAdapterOnScanningStopped(object? sender, EventArgs e)
        {
            ScanningStopped?.Raise(this, e);
        }

        private void ActiveAdapterOnScanErrorEncountered(object? sender, EventArgs e)
        {
            ScanErrorEncountered?.Raise(this, e);
        }

        private void ActiveAdapterOnEncoderUpdated(object? sender, EncoderUpdateArgs e)
        {
            EncoderUpdated?.Raise(this, e);
        }

        private void ActiveAdapterOnMessageReceived(object? sender, PluginMessageEventArgs e)
        {
            // re-broadcast
            PluginMessageReceived?.Raise(sender, e);
            
            PluginMessages.Insert(0,new UserMessage(sender,e));
            if (PluginMessages.Count > maxUserMessages)
            {
                PluginMessages.RemoveAt(maxUserMessages);
            }
        }

        #endregion

        #region Lifecycle

        public static LogScannerEngine Create(string? configPath = null)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<CoreModule>();
            var container = builder.Build();
            using var scope = container.BeginLifetimeScope();
            if (!string.IsNullOrEmpty(configPath))
            {
                var configLocator = scope.Resolve<IConfigLocator>();
                configLocator.OverrideDefaultConfigLocation(configPath);
            }
            return scope.Resolve<LogScannerEngine>();
        }
        
        public LogScannerEngine(
            IEnumerable<IScannerAdapter> availableAdapters,
            IFlightsAndWindowFilter filter,
            ILogger logger,
            ILogAssembler logAssembler,
            ILogArchiver archiver,
            LogModelBuilder modelBuilder,
            RawProfileDumper dumper,
            IEnumerable<ILogModelConsumerPlugin> consumers,
            CoreConfig config
        )
        {
            
            this.archiver = archiver;
            this.dumper = dumper;
            // this is a bit hacky, I would rather have a separate list of registered IHeartBeatSubscribers,
            // but I couldn't figure out how to resolve them. This works.
            heartBeatSubscribers = consumers!.ToList().OfType<IHeartBeatSubscriber>().ToList();

            Filter = filter;
            Config = config;
            this.availableAdapters = availableAdapters;
            ActiveAdapter = null;
            Logger = logger;
            Logger.Debug($"LogScannerEngine {GitVersionInformation.FullSemVer}");
            LogAssembler = logAssembler;
            ModelBuilder = modelBuilder;
            ModelBuilder.ModelingFailed += (_, _) =>
            {
                Task.Run(dumper.DumpHistory).ConfigureAwait(false);
            };
            Consumers = consumers;

            foreach (var a in AvailableAdapters)
            {
                logger.Debug($"Available Adapter: {a.Name} ");
            }

            SetupPipeline();
            // set up the background task that every 5 seconds checks the adapter
            if (heartBeatSubscribers.Any())
            {
                RecurringTaskHelper.RecurringTask(CheckScanningStatus,
                    heartBeatSubscribers.Min(q => q!.RequestedInterval), statusCheckerSource.Token);
            }
        }

        private void SetupPipeline()
        {
            var blockOptions = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 3, EnsureOrdered = true };
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };


            var unitConverterBlock = new TransformBlock<Profile, Profile>((p) =>
                UnitConverter.Convert(ActiveAdapter.Units, UnitSystem.Millimeters, p));
            // dumper.DumpBlock is a pass-through from the source block where the profiles originate, scannerAdapter.AvailableProfiles
            dumper.DumpBlock.LinkTo(unitConverterBlock, linkOptions);
            var boundingBoxBlock = new TransformBlock<Profile, Profile>(BoundingBox.UpdateBoundingBox, blockOptions);
            unitConverterBlock.LinkTo(boundingBoxBlock, linkOptions);
            // then we transform profiles by using a flights-and-window filter 
            var filterTransformBlock = new TransformBlock<Profile, Profile>(Filter.Apply, blockOptions);
            // the output of the bounding box block is linked to the filter block
            boundingBoxBlock.LinkTo(filterTransformBlock, linkOptions);
            // the engine also has a broadcast block, basically a tee that distributes all incoming 
            // profiles to all connected further processing steps
            // in our case, we use it for distributing the profiles both to the assembler as well as to 
            // the UI components that want a live stream
            filterTransformBlock.LinkTo(RawProfilesBroadcastBlock, linkOptions);
            // end the pipeline by feeding the profiles to the log assembler
            var pipelineEndBlock = new ActionBlock<Profile>(FeedToAssembler,
                new ExecutionDataflowBlockOptions() { EnsureOrdered = true, MaxDegreeOfParallelism = 1 });
            RawProfilesBroadcastBlock.LinkTo(pipelineEndBlock);

            // next pipeline is for RawLogs, we have the BufferBlock RawLogs from the assembler for that
            LogAssembler.RawLogs.LinkTo(RawLogsBroadcastBlock);
            // the archiver gets to see all raw logs
            RawLogsBroadcastBlock.LinkTo(new ActionBlock<RawLog>((l) => archiver.ArchiveLog(l)));
            // the raw logs get sent to ModelBuilder. 
            RawLogsBroadcastBlock.LinkTo(ModelBuilder.BuilderBlock);

            ModelBuilder.BuilderBlock.LinkTo(LogModelBroadcastBlock);
            // the LogModelBroadcastBlock receives finished LogModels and now the end user can subscribe to 
            // it and do any processing needed, like a sorter or sending to an optimizer
            foreach (var logModelConsumer in Consumers)
            {
                logModelConsumer.PluginMessage += ActiveAdapterOnMessageReceived;
                logModelConsumer.Initialize();
                if (logModelConsumer.IsInitialized)
                {
                    // TODO: check for version and GUID here
                    var userBlock = new ActionBlock<LogModelResult>(logModelConsumer.Consume);
                    LogModelBroadcastBlock.LinkTo(userBlock);
                }
            }
        }

        #endregion


        #region Runtime

        public void SetActiveAdapter(string name)
        {
            var adapter = availableAdapters.FirstOrDefault(q => q.Name == name);
            if (adapter == null)
            {
                var msg = $"Adapter \"{name}\" not found.";
                Logger.Warn(msg);
                throw new ApplicationException(msg);
            }

            SetActiveAdapter(adapter);
        }

        public void SetActiveAdapter(IScannerAdapter adapter)
        {
            if (IsRunning)
            {
                var msg = "Cannot change adapters while running.";
                Logger.Warn(msg);
                throw new ApplicationException(msg);
            }

            if (adapter == ActiveAdapter)
            {
                return;
            }

            if (ActiveAdapter != null)
            {
                // unhook events
                ActiveAdapter.ScanningStarted -= ActiveAdapterOnScanningStarted;
                ActiveAdapter.ScanningStopped -= ActiveAdapterOnScanningStopped;
                ActiveAdapter.ScanErrorEncountered -= ActiveAdapterOnScanErrorEncountered;
                ActiveAdapter.EncoderUpdated -= ActiveAdapterOnEncoderUpdated;
                ActiveAdapter.PluginMessage -= ActiveAdapterOnMessageReceived;
                // unlinker is a Disposable that represents the link from the ActiveAdapter - deleting it 
                // unlinks the current adapter from the pipeline start
                unlinker!.Dispose();
            }


            ActiveAdapter = adapter;
            if (ActiveAdapter != null)
            {
                Logger.Info($"Setting active adapter to {ActiveAdapter.Name}");
                // hook up to new adapter
                ActiveAdapter.ScanningStarted += ActiveAdapterOnScanningStarted;
                ActiveAdapter.ScanningStopped += ActiveAdapterOnScanningStopped;
                ActiveAdapter.ScanErrorEncountered += ActiveAdapterOnScanErrorEncountered;
                ActiveAdapter.EncoderUpdated += ActiveAdapterOnEncoderUpdated;
                ActiveAdapter.PluginMessage += ActiveAdapterOnMessageReceived;
                // entry point, the AvailableProfiles is the source of all profiles. 
                unlinker = ActiveAdapter.AvailableProfiles.LinkTo(dumper.DumpBlock,
                    new DataflowLinkOptions { PropagateCompletion = true });
                // dumper.IsEnabled = !ActiveAdapter!.IsReplay;
            }
            else
            {
                return;
            }

            OnAdapterChanged();
        }

        public async Task<bool> Start()
        {
            CheckForActiveAdapter();
            var msg = string.Empty;
            try
            {
                if (await ActiveAdapter!.ConfigureAsync())
                {
                    await ActiveAdapter.StartAsync();
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }

            return true;
        }

        public void Stop()
        {
            CheckForActiveAdapter();

            try
            {
                ActiveAdapter!.Stop();
            }
            catch (Exception e)
            {
                var msg = e.Message;
                Logger.Debug(msg);
            }
        }

        public void StartDumping()
        {
            dumper.StartDumping();
//notifier.Success("Now dumping raw profiles to disk.");
        }

        public void StopDumping()
        {
            dumper.StopDumping();
            //          notifier.Success("Stopped dumping raw profiles.");
        }

        #endregion

        #region Private Methods

        private void FeedToAssembler(Profile profile)
        {
            LogAssembler.AddProfile(profile);
        }

        private void CheckForActiveAdapter()
        {
            if (ActiveAdapter == null)
            {
                string msg = "No active adapter set.";
                Logger.Error(msg);
                throw new ApplicationException(msg);
            }
        }

        private void CheckScanningStatus()
        {
            foreach (var heartBeatSubscriber in heartBeatSubscribers)
            {
                heartBeatSubscriber.Callback(ActiveAdapter is { IsRunning: true });
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            unlinker?.Dispose();
            statusCheckerSource.Cancel();
        }

        #endregion
    }
}
