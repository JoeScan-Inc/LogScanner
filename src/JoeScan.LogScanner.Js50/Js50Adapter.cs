using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.Pinchot;
using NLog;
using System.Threading.Tasks.Dataflow;
using Profile = JoeScan.LogScanner.Core.Models.Profile;

namespace JoeScan.LogScanner.Js50;

public class Js50Adapter : IScannerAdapter
{
    public IJs50AdapterConfig Config { get; }
    private readonly ILogger? logger;
    private ScanSystem? scanSystem;
    private CancellationTokenSource? cancellationTokenSource;
    private Thread? scanThread;
    private bool isRunning;
    static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
    private readonly ScanSyncReceiverThread encoderUpdater;
    private const int maxStartupTimeS = 10;

    #region Lifecycle

    public Js50Adapter(ILogger logger, IJs50AdapterConfig config, ScanSyncReceiverThread encoderUpdater)
    {
        Config = config;
        this.logger = logger;
        this.encoderUpdater = encoderUpdater;
        logger.Debug($"Created Js50Adapter using JoeScan Pinchot API version {Pinchot.VersionInformation.Version}");
        Units = UnitSystem.Inches;
        encoderUpdater.EventUpdateFrequency = 100;
        encoderUpdater.ScanSyncUpdate += EncoderUpdaterOnScanSyncUpdate;
    }

    #endregion

    #region IScannerAdapter Implementation
    public string Name => $"JS-50 (Pinchot v{Pinchot.VersionInformation.Version})";
    public UnitSystem Units { get; }
    public bool IsConfigured => true;

    // If we give a finite BoundedCapacity, the BufferBlock will discard Profiles 
    // i.e Post() will return false. -1 means unlimited buffering - hopefully the 
    // subsequent steps are fast enough not to make this a memory hog
    public BufferBlock<Profile> AvailableProfiles { get; } =
        new BufferBlock<Profile>(new DataflowBlockOptions() { BoundedCapacity = -1 });

    public void Configure()
    {
        try
        {
            encoderUpdater.Start();
            logger!.Debug("Started ScanSyncReceiverThread.");
        }
        catch (Exception e)
        {
            logger!.Error($"Could not start ScanSyncReceiverThread: {e.Message}");
        }
    }

    public void Start()
    {
        logger!.Debug($"Trying to start {this.GetType().Name}.");
        if (!IsConfigured)
        {
            throw new ApplicationException(
                $"{this.GetType().Name} not configured. You need to call Configure() before calling Start().");
        }
        if (!IsRunning)
        {
            scanSystem = SetupScanSystem();
            if (scanSystem != null)
            {
                // kick off scan thread
                cancellationTokenSource = new CancellationTokenSource();
                ThreadStart threadMainStart = delegate { ScanLoop(cancellationTokenSource.Token); };
                scanThread = new Thread(threadMainStart) { Priority = ThreadPriority.Normal, IsBackground = true };
                scanThread.Start();
                if (!autoResetEvent.WaitOne(TimeSpan.FromSeconds(maxStartupTimeS)))
                {
                    string msg = "Timed out connecting.";
                    logger.Error(msg);
                    throw new ApplicationException(msg);
                }
                IsRunning = true;
            }
            else
            {
                string msg = "Could not create ScanSystem.";
                logger.Error(msg);
                throw new ApplicationException(msg);
            }
        }
        else
        {
            string msg = "Failed to Start: adapter already running.";
            logger.Error(msg);
            throw new ApplicationException(msg);
        }
    }

    private void ScanLoop(CancellationToken ct)
    {
        try
        {
            var timeOut = TimeSpan.FromSeconds(1);
            logger!.Debug($"Attempting to connect to scan heads with timeout of {timeOut}.");
            var disconnectedHeads = scanSystem!.Connect(timeOut);
            if (disconnectedHeads == null)
            {
                throw new InvalidOperationException("ScanSystem.Connect() call timed out.");
            }
            if (disconnectedHeads.Any())
            {
                foreach (var scanHead in disconnectedHeads)
                {
                    logger.Error($"Scan head {scanHead.SerialNumber} did not respond to connection request.");
                }
                throw new InvalidOperationException($"Not starting scan thread due to connection error.");
            }
            logger.Debug($"All scan heads connected.");
            foreach (var scanHead in scanSystem.ScanHeads)
            {
                logger.Debug($"Active scan head: {scanHead.SerialNumber} ({scanHead.ID}) FW: {scanHead.Status.FirmwareVersion.Version}");
            }

            logger.Debug($"Using DataFormat {Config.DataFormat}.");
            var systemMaxScanRate = scanSystem!.GetMaxScanRate();
            logger!.Debug(
                $"ScanSystem reported a Max Scan Rate of {systemMaxScanRate} Hz (min Scan Period: {1000.0 / systemMaxScanRate:F2} ms)");
            logger.Debug($"Configuration requested ScanRate is {Config.ScanRate} Hz (min Scan Period: {1000.0 / Config.ScanRate:F2} ms)");
            if (Config.ScanRate > systemMaxScanRate)
            {
                logger.Warn($"Configuration requested rate ({Config.ScanRate} Hz) is higher than the system max rate ({systemMaxScanRate} Hz) - using system max rate.");
            }
            scanSystem.StartScanning(Config.ScanRate > systemMaxScanRate ? systemMaxScanRate : Config.ScanRate, Config.DataFormat);
            // we seem to have connected and are scanning
            autoResetEvent.Set();
            while (!ct.IsCancellationRequested)
            {
                // post all profiles that are due
                ct.ThrowIfCancellationRequested();
                foreach (var scanHead in scanSystem.ScanHeads)
                {
                    var prof = scanHead.TakeNextProfile(ct);
                    if (prof != null)
                    {
                        AvailableProfiles.Post(prof.ToLogScannerProfile());
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // perfectly normal exception we get when using the token to cancel
            AvailableProfiles.Complete();
        }
        catch (Exception e)
        {
            string msg = $"Encountered scanning error: {e}";
            logger!.Error(msg);
            OnScanErrorEncountered();
        }
        finally
        {
            if (scanSystem!.IsScanning)
            {
                scanSystem.StopScanning();
            }
            IsRunning = false;
        }
    }

    public Task StartAsync()
    {
        return Task.Run(Start);
    }

    public void Stop()
    {
        logger!.Debug($"Trying to stop {this.GetType().Name}.");
        if (!IsRunning)
        {
            return;
        }
        cancellationTokenSource!.Cancel();
        if (!scanThread!.Join(TimeSpan.FromSeconds(1)))
        {
           logger.Warn("ScanThread did not exit. Abandoning it.");
        }
        else
        {
            logger.Debug("Clean shutdown of scan thread successful.");
        }

        scanThread = null;
        cancellationTokenSource = null;
    }

    public Task StopAsync()
    {
        return Task.Run(Stop);
    }

    public bool IsRunning
    {
        get => isRunning;
        set
        {
            if (value != isRunning)
            {
                isRunning = value;
                if (isRunning)
                {
                    OnScanningStarted();
                }
                else
                {
                    OnScanningStopped();
                }
            }
        }
    }

    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;
    public bool IsReplay => false;

    #endregion

    private ScanSystem SetupScanSystem()
    {
        logger!.Debug("Setting up ScanSystem");
        var system = new ScanSystem();
        logger.Debug($"Configuration contains {Config.ScanHeads.Count()} heads.");

        try
        {
            foreach (var headConfig in Config.ScanHeads)
            {
                logger.Debug($"Creating ScanHead for serial number {headConfig.Serial} with ID {headConfig.Id}.");
                var scanHead = system.CreateScanHead(headConfig.Serial, headConfig.Id);
                var conf = new ScanHeadConfiguration();
                logger.Debug($"Configuring head {scanHead.ID}.");
                logger.Debug($"Setting Laser Exposure for {scanHead.ID} " +
                             $"to {headConfig.MinLaserOn}/{headConfig.DefaultLaserOn}/{headConfig.MaxLaserOn} "
                             + "(min/default/max)");
                conf.SetLaserOnTime(headConfig.MinLaserOn, headConfig.DefaultLaserOn, headConfig.MaxLaserOn);
                logger.Debug($"Setting Scan Phase Offset for {scanHead.ID} to {headConfig.ScanPhaseOffset} µs");
                conf.ScanPhaseOffset = headConfig.ScanPhaseOffset;
                logger.Debug($"Applying configuration to {scanHead.ID}");
                scanHead.Configure(conf);
                logger.Debug($"Setting Window for {scanHead.ID} to {headConfig.WindowTop}/"
                             + $"{headConfig.WindowBottom}/"
                             + $"{headConfig.WindowLeft}/"
                             + $"{headConfig.WindowRight}"
                             + "(Top/Bottom/Left/Right");
                scanHead.SetWindow(ScanWindow.CreateScanWindowRectangular(headConfig.WindowTop,
                    headConfig.WindowBottom,
                    headConfig.WindowLeft,
                    headConfig.WindowRight));
                logger.Debug($"Setting Alignment for head {scanHead.ID} to ShiftX: {headConfig.AlignmentShiftX}" 
                + $" ShiftY: {headConfig.AlignmentShiftY} "
                + $" RollDeg: {headConfig.AlignmentRollDegrees}"
                + $" Orientation: {headConfig.AlignmentOrientation}");
                scanHead.SetAlignment(headConfig.AlignmentRollDegrees,
                    headConfig.AlignmentShiftX,
                    headConfig.AlignmentShiftY,
                    headConfig.AlignmentOrientation);
            }
            logger.Debug("Done setting up ScanSystem");
            return system;
        }
        catch (Exception e)
        {
            logger.Error($"Failed to create ScanSystem: {e.Message}");
            throw;
        }
    }

    protected virtual void OnScanningStarted()
    {
        ScanningStarted?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnScanningStopped()
    {
        ScanningStopped?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnScanErrorEncountered()
    {
        ScanErrorEncountered?.Invoke(this, EventArgs.Empty);
    }

   

    private void EncoderUpdaterOnScanSyncUpdate(object? sender, EncoderUpdateArgs e)
    {
        // we just pass on the event
        EncoderUpdated.Raise(this, e);
    }
}
