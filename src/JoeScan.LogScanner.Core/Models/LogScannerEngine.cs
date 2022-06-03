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
        public IFlightsAndWindowFilter Filter { get; }
        public ICoreConfig Config { get; }
        public IScannerAdapter ScannerAdapter { get; }
        public ILogger Logger { get; }
        public ILogAssembler LogAssembler { get; }
        public BroadcastBlock<Profile> RawProfiles { get; }
        public BroadcastBlock<RawLog> RawLogs { get; } = new BroadcastBlock<RawLog>(r => r);
        public UnitSystem Units { get; }
        
        #region Event Handlers

        public event EventHandler ScanningStarted;
        public event EventHandler ScanningStopped;
        public event EventHandler ScanErrorEncountered;
        public event EventHandler EncoderUpdated;

        #endregion

        #region Event Invocation

        private void OnScanErrorEncountered(EventArgs e)
        {
            ScanErrorEncountered?.Invoke(this, e);
        }

        private void OnScanningStarted(EventArgs e)
        {
            ScanningStarted?.Invoke(this, e);

        }

        private void OnScanningStopped(EventArgs e)
        {
            ScanningStopped?.Invoke(this, e);
        }

        private void OnEncoderUpdated(EncoderUpdateArgs e)
        {
            EncoderUpdated?.Invoke(this, e);
        }

        #endregion

        #region Lifecycle

        public LogScannerEngine(ICoreConfig config,
            IScannerAdapter scannerAdapter,
            IFlightsAndWindowFilter filter,
            ILogger logger,
            ILogAssembler logAssembler,
            IUserNotifier notifier)
        {
            this.notifier = notifier;
            Filter = filter;
            Config = config;
            ScannerAdapter = scannerAdapter;
            Logger = logger;
            LogAssembler = logAssembler;

            //TODO: make this more robust
            Units = Config.Units;

            ScannerAdapter.ScanningStarted += (sender, args) => OnScanningStarted(args);
            ScannerAdapter.ScanningStopped += (sender, args) => OnScanningStopped(args);
            ScannerAdapter.ScanErrorEncountered += (sender, args) => OnScanErrorEncountered(args);
            ScannerAdapter.EncoderUpdated += (sender, args) => OnEncoderUpdated(args as EncoderUpdateArgs);

            // set up processing pipeline, we can do all this in parallel
            var blockOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 3,

            };
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            //  a block that modifies the Profiles with a bounding box - it goes first if the units dont't need to 
            // be converted, and second if they do
            var boundingBoxBlock = new TransformBlock<Profile, Profile>(BoundingBox.UpdateBoundingBox, blockOptions);
            // only create a unit converter block if the units in the adapter are different from
            // the engine units
            if (Units != ScannerAdapter.Units)
            {
                var unitConverterBlock =
                    new TransformBlock<Profile, Profile>((p) => UnitConverter.Convert(ScannerAdapter.Units, Units, p));
                // scannerAdapter.AvailableProfiles is the source block where the profiles originate
                scannerAdapter.AvailableProfiles.LinkTo(unitConverterBlock, linkOptions);
                unitConverterBlock.LinkTo(boundingBoxBlock, linkOptions);
            }
            else
            {
                // scannerAdapter.AvailableProfiles is the source block where the profiles originate
                scannerAdapter.AvailableProfiles.LinkTo(boundingBoxBlock, linkOptions);
            }

            // then we transform profiles by using a flights-and-window filter 
            var filterTransformBlock = new TransformBlock<Profile, Profile>(filter.Apply, blockOptions);
            // the output of the bounding box block is linked to the filter block
            boundingBoxBlock.LinkTo(filterTransformBlock, linkOptions);
            // the engine also has a broadcast block, basically a tee that distributes all incoming 
            // profiles to all connected further processing steps
            // in our case, we use it for distributing the profiles both to the assembler as well as to 
            // the UI components that want a live stream
            RawProfiles = new BroadcastBlock<Profile>(profile => profile);
            filterTransformBlock.LinkTo(RawProfiles, linkOptions);
            // end the pipeline by feeding the profiles to the log assembler
            var pipelineEndBlock = new ActionBlock<Profile>(FeedToAssembler);
            RawProfiles.LinkTo(pipelineEndBlock);


            // next pipeline is for RawLogs, we have the BufferBlock RawLogs from the assembler for that
            LogAssembler.RawLogs.LinkTo(RawLogs);
        }
        #endregion

        #region Private Methods

        private void FeedToAssembler(Profile profile)
        {
            LogAssembler.AddProfile(profile);
        }

        #endregion

        #region Runtime

        public async void Start()
        {
            notifier.IsBusy = true;
            string msg = string.Empty;
            try
            {
                ScannerAdapter.Configure();
                await ScannerAdapter.StartAsync();
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
            notifier.IsBusy = true;
            string msg = string.Empty;
            try
            {
                await ScannerAdapter.StopAsync();
                notifier.IsBusy = false;
                notifier.Success("Stopped Scanning.");
            }
            catch (Exception e)
            {
                notifier.IsBusy = false;
                msg = e.Message;
                notifier.Error($"Problem stopping. Error was : {msg}");
            }
        }

        public bool IsRunning => ScannerAdapter.IsRunning;

        #endregion
    }
}
