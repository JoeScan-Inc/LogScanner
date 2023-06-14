using Autofac.Features.AttributeFilters;
using Config.Net;
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
    #region Private Fields

    private ScanSystem? scanSystem;
    private CancellationTokenSource? cancellationTokenSource;
    private Thread? scanThread;
    private bool isRunning;
    static readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
    private const int maxStartupTimeS = 10;

    #endregion

    #region Injected

    private readonly ILogger? logger;
    private IJs50AdapterConfig Config { get; }
    private readonly ScanSyncReceiverThread encoderUpdater;

    #endregion

    #region Lifecycle

    public Js50Adapter(ILogger logger)
    {
        Config = new ConfigurationBuilder<IJs50AdapterConfig>()
            .UseJsonFile("js50adapter.json")
            .Build();
        this.logger = logger;
        encoderUpdater = new ScanSyncReceiverThread(logger);
        logger.Debug($"Created Js50Adapter using JoeScan Pinchot API version {Pinchot.VersionInformation.Version}");
        Units = UnitSystem.Inches;
        encoderUpdater.EventUpdateFrequencyMs = 100;
        encoderUpdater.ScanSyncUpdate += EncoderUpdaterOnScanSyncUpdate;
    }

    #endregion

    #region IScannerAdapter Implementation
    public string Name => $"JS-50";
    public UnitSystem Units { get; }
    public bool IsConfigured { get; private set; }


    // If we give a finite BoundedCapacity, the BufferBlock will discard Profiles 
    // i.e Post() will return false. -1 means unlimited buffering - hopefully the 
    // subsequent steps are fast enough not to make this a memory hog
    public BufferBlock<Profile> AvailableProfiles { get; } =
        new BufferBlock<Profile>(new DataflowBlockOptions() { BoundedCapacity = -1 });

    public Task<bool> ConfigureAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                encoderUpdater.Start();
                IsConfigured = true;
                logger!.Debug("Started ScanSyncReceiverThread.");
            }
            catch (Exception e)
            {
                IsConfigured = false;
                logger!.Error($"Could not start ScanSyncReceiverThread: {e.Message}");
            }

            return IsConfigured;
        });

    }

    // TODO: temporary fix, make Start properly Async
    public Task<bool> StartAsync()
    {
        return Task.Run((() =>
        {
            Start();
            return IsRunning;
        }));
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

        if (scanSystem != null)
        {
            scanSystem.Dispose();
            scanSystem = null;
        }
        scanThread = null;
        cancellationTokenSource = null;
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
    public event EventHandler<PluginMessageEventArgs>? PluginMessage;

    public bool IsReplay => false;
    public uint VersionMajor => 1;
    public uint VersionMinor => 0;
    public uint VersionPatch => 0;
    public Guid Id { get; } = Guid.Parse("{D1021E6A-7C7C-43F0-9444-303FDCAE2BFF}");

    #endregion

    private void ScanLoop(CancellationToken ct)
    {
        try
        {
            var timeOut = TimeSpan.FromSeconds(10);
            logger!.Debug($"Attempting to connect to scan heads with timeout of {timeOut} seconds.");
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
                logger.Debug($"Active scan head: {scanHead.SerialNumber} ({scanHead.ID}) ");
            }

            logger.Debug($"Using DataFormat {Config.DataFormat}.");
            var minScanPeriod = scanSystem!.GetMinScanPeriod();
            logger!.Debug(
                $"ScanSystem reported a MinScanPeriod of {minScanPeriod} μs)");
           // logger.Debug($"Configuration requested ScanRate is {Config.ScanRate} Hz (min Scan Period: {1000.0 / Config.ScanRate:F2} ms)");
            // if (Config.ScanRate > minScanPeriod)
            // {
            //     logger.Warn($"Configuration requested rate ({Config.ScanRate} Hz) is higher than the system max rate ({minScanPeriod} Hz) - using system max rate.");
            // }
            scanSystem.StartScanning(minScanPeriod+100, Config.DataFormat);
            int failedToPost = 0;
            // we seem to have connected and are scanning
            autoResetEvent.Set();
            while (!ct.IsCancellationRequested)
            {
                // post all profiles that are due
                ct.ThrowIfCancellationRequested();
                foreach (var scanHead in scanSystem.ScanHeads)
                {
                    var gotProfile = scanHead.TryTakeNextProfile(out var prof, TimeSpan.FromMilliseconds(1), ct);
                    if (gotProfile)
                    {
                        //TODO: check the result of Post to see if we lose profiles
                        // due to the downstream processing being too slow
                        if (!AvailableProfiles.Post(prof.ToLogScannerProfile()))
                        {
                            failedToPost++;
                            if (failedToPost >= 100)
                            {
                                string msg = "BufferBlock failed to post new profiles 100 times.";
                                logger.Error(msg);
                                throw new InternalBufferOverflowException(msg);
                            }
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // perfectly normal exception we get when using the token to cancel
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

            scanSystem.Dispose();
            scanSystem = null;

            IsRunning = false;
        }
    }

    private ScanSystem SetupScanSystem()
    {
        logger!.Debug("Setting up ScanSystem");
        var system = new ScanSystem(ScanSystemUnits.Inches);
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
                conf.SetLaserOnTime((uint)headConfig.MinLaserOn,(uint) headConfig.DefaultLaserOn, (uint)headConfig.MaxLaserOn);
                
                
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
                    headConfig.AlignmentShiftY
                    );
                system.AddPhase();
                system.AddPhaseElement(headConfig.Id,Camera.CameraA);
                system.AddPhaseElement(headConfig.Id,Camera.CameraB);
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

    protected virtual void OnAdapterMessage(PluginMessageEventArgs e)
    {
        PluginMessage?.Invoke(this, e);
    }
}
