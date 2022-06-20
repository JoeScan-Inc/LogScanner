using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Models
{
    public class LogScannerEngine
    {
        private readonly IUserNotifier notifier;
        private RawProfileDumper dumper;
        public IFlightsAndWindowFilter Filter { get; }
        public ICoreConfig Config { get; }
        private readonly IEnumerable<IScannerAdapter> availableAdapters;
        public IReadOnlyList<IScannerAdapter> AvailableAdapters => new List<IScannerAdapter>(availableAdapters);
        public IScannerAdapter? ActiveAdapter { get; private set; }
        private ILogger Logger { get; }
        public ILogAssembler LogAssembler { get; }
        public BroadcastBlock<Profile> RawProfiles { get; private set; } 
            = new BroadcastBlock<Profile>(profile => profile);
        public BroadcastBlock<RawLog> RawLogs { get; } 
            = new BroadcastBlock<RawLog>(r => r);
        public UnitSystem Units { get; }
        public bool IsRunning => ActiveAdapter is { IsRunning: true };
        public bool CanStart => ActiveAdapter != null && !IsRunning;

        #region Event Handlers

        public event EventHandler ScanningStarted;
        public event EventHandler ScanningStopped;
        public event EventHandler ScanErrorEncountered;
        public event EventHandler EncoderUpdated;
        public event EventHandler AdapterChanged;

        #endregion

        #region Event Invocation

        protected virtual void OnAdapterChanged()
        {
            AdapterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ActiveAdapterOnScanningStarted(object? sender, EventArgs e)
        {
            ScanningStarted?.Invoke(this, e);
        }

        private void ActiveAdapterOnScanningStopped(object? sender, EventArgs e)
        {
            ScanningStopped?.Invoke(this,e);
        }

        private void ActiveAdapterOnScanErrorEncountered(object? sender, EventArgs e)
        {
            ScanErrorEncountered?.Invoke(this, e);
        }

        private void ActiveAdapterOnEncoderUpdated(object? sender, EncoderUpdateArgs e)
        {
            EncoderUpdated?.Invoke(this, e);
        }

        #endregion

        #region Lifecycle

        public LogScannerEngine(ICoreConfig config,
            IEnumerable<IScannerAdapter> availableAdapters,
            IFlightsAndWindowFilter filter,
            ILogger logger,
            ILogAssembler logAssembler,
            IUserNotifier notifier)
        {
            this.notifier = notifier;
            Filter = filter;
            Config = config;
            this.availableAdapters = availableAdapters;
            ActiveAdapter = null;
            Logger = logger;
            LogAssembler = logAssembler;
            Units = Config.Units;
            foreach (var a in AvailableAdapters)
            {
                 logger.Debug($"Available Adapter: {a.Name} ");
            }
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
            }

            ActiveAdapter = adapter;
            // hook up to new adapter
            ActiveAdapter.ScanningStarted += ActiveAdapterOnScanningStarted;
            ActiveAdapter.ScanningStopped += ActiveAdapterOnScanningStopped;
            ActiveAdapter.ScanErrorEncountered += ActiveAdapterOnScanErrorEncountered;
            ActiveAdapter.EncoderUpdated += ActiveAdapterOnEncoderUpdated;

            // set up processing pipeline, we can do all this in parallel
            var blockOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 3,

            };
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            dumper = new RawProfileDumper(Logger);
            // entry point, the AvailableProfiles is the source of all profiles
            ActiveAdapter.AvailableProfiles.LinkTo(dumper.DumpBlock, linkOptions);
            //  a block that modifies the Profiles with a bounding box - it goes first if the units dont't need to 
            // be converted, and second if they do
            var boundingBoxBlock = new TransformBlock<Profile, Profile>(BoundingBox.UpdateBoundingBox, blockOptions);
            // only create a unit converter block if the units in the adapter are different from
            // the engine units
            if (Units != ActiveAdapter.Units)
            {
                var unitConverterBlock =
                    new TransformBlock<Profile, Profile>((p) => UnitConverter.Convert(ActiveAdapter.Units, Units, p));
                // dumper.DumpBlock is a pass-through from the source block where the profiles originate, scannerAdapter.AvailableProfiles
                dumper.DumpBlock.LinkTo(unitConverterBlock, linkOptions);
                unitConverterBlock.LinkTo(boundingBoxBlock, linkOptions);
            }
            else
            {
                // dumper.DumpBlock is a pass-through from the source block where the profiles originate, scannerAdapter.AvailableProfiles
                dumper.DumpBlock.LinkTo(boundingBoxBlock, linkOptions);
            }

            // then we transform profiles by using a flights-and-window filter 
            var filterTransformBlock = new TransformBlock<Profile, Profile>(Filter.Apply, blockOptions);
            // the output of the bounding box block is linked to the filter block
            boundingBoxBlock.LinkTo(filterTransformBlock, linkOptions);
            // the engine also has a broadcast block, basically a tee that distributes all incoming 
            // profiles to all connected further processing steps
            // in our case, we use it for distributing the profiles both to the assembler as well as to 
            // the UI components that want a live stream
            filterTransformBlock.LinkTo(RawProfiles, linkOptions);
            // end the pipeline by feeding the profiles to the log assembler
            var pipelineEndBlock = new ActionBlock<Profile>(FeedToAssembler);
            RawProfiles.LinkTo(pipelineEndBlock);
            
            // next pipeline is for RawLogs, we have the BufferBlock RawLogs from the assembler for that
            LogAssembler.RawLogs.LinkTo(RawLogs);
            //TODO: read from config
            dumper.OutputDir = "C:\\tmp\\rawdumps";
            OnAdapterChanged();
        }

        public async void Start()
        {
            CheckForActiveAdapter();
            notifier.IsBusy = true;
            var msg = string.Empty;
            try
            {
                ActiveAdapter!.Configure();
                await ActiveAdapter.StartAsync();
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            finally
            {
                notifier.IsBusy = false;
            }
            if (IsRunning)
            {
                notifier.Success("Successfully Started Scanning.");
            }
            else
            {
                notifier.Error($"Failed to start scanning. Error was : {msg}");
            }
        }

        public async void Stop()
        {
            CheckForActiveAdapter();
            notifier.IsBusy = true;
            
            try
            {
                await ActiveAdapter!.StopAsync();
                notifier.IsBusy = false;
                notifier.Success("Stopped Scanning.");
            }
            catch (Exception e)
            {
                notifier.IsBusy = false;
                var msg = e.Message;
                Logger.Debug(msg);
                notifier.Error($"Problem stopping. Error was : {msg}");
            }
        }

        public void StartDumping()
        {
            dumper.StartDumping();
            notifier.Success("Now dumping raw profiles to disk.");
        }

        public void StopDumping()
        {
            dumper.StopDumping();
            notifier.Success("Stopped dumping raw profiles.");
        }

        #endregion
    }
}
