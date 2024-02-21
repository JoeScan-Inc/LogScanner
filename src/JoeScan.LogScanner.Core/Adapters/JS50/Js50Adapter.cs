using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.Pinchot;
using NLog;
using System.Threading.Tasks.Dataflow;
using Profile = JoeScan.LogScanner.Core.Models.Profile;

namespace JoeScan.LogScanner.Core.Adapters.JS50;

public  class Js50Adapter : AdapterBase, IScannerAdapter
{
    private readonly IConfigLocator configLocator;
    #region Private Fields

    private ScanSystem? scanSystem;
    private CancellationTokenSource? cancellationTokenSource;
    private Thread? scanThread;
    private bool isRunning;
    static readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
    private const int maxStartupTimeS = 10;

    #endregion

    #region Injected

    private IJs50AdapterConfig Config { get; }

    #endregion

    #region Lifecycle

    public  Js50Adapter(ILogger logger, IJs50AdapterConfig config, IConfigLocator configLocator)
    : base(logger)
    {
        this.configLocator = configLocator;
        Config = config;

        var msg = $"Created Js50Adapter using JoeScan Pinchot API version {JoeScan.Pinchot.VersionInformation.Version}";
        DiagnosticMessage(msg, LogLevel.Info);
        Units = UnitSystem.Millimeters;
        // encoderUpdater.EventUpdateFrequencyMs = 100;
        // encoderUpdater.ScanSyncUpdate += EncoderUpdaterOnScanSyncUpdate;
    }

    #endregion

    #region IScannerAdapter Implementation
    public string Name => $"JS-50 v16.2.x";
    public UnitSystem Units { get; } = UnitSystem.Millimeters;
    public bool IsConfigured { get; private set; }


    // If we give a finite BoundedCapacity, the BufferBlock will discard Profiles 
    // i.e Post() will return false. -1 means unlimited buffering - hopefully the 
    // subsequent steps are fast enough not to make this a memory hog
    public BufferBlock<Profile> AvailableProfiles { get; } =
        new BufferBlock<Profile>(new DataflowBlockOptions() { BoundedCapacity = -1 });

    public Task<bool> ConfigureAsync()
    {
        IsConfigured = true;
        return Task.FromResult(true);
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
        DiagnosticMessage($"Trying to start {this.GetType().Name}.",LogLevel.Info);
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
                    string msg = "ScanThread did not start in time. Abandoning it.";
                    DiagnosticMessage(msg, LogLevel.Error);
                    throw new ApplicationException(msg);
                }
                IsRunning = true;
            }
            else
            {
                string msg = "Could not create ScanSystem.";
              
                DiagnosticMessage(msg, LogLevel.Error);
                throw new ApplicationException(msg);
            }
        }
        else
        {
            string msg = "Failed to Start: adapter already running.";
            DiagnosticMessage(msg, LogLevel.Error);
            throw new ApplicationException(msg);
        }
    }



    public void Stop()
    {
        var msg = $"Trying to stop {this.GetType().Name}.";
        DiagnosticMessage(msg, LogLevel.Info);
        if (!IsRunning)
        {
            return;
        }
        cancellationTokenSource!.Cancel();
        if (!scanThread!.Join(TimeSpan.FromSeconds(1)))
        {
            DiagnosticMessage("ScanThread did not exit. Abandoning it.", LogLevel.Warn);
        }
        else
        {
            DiagnosticMessage("ScanThread exited cleanly.", LogLevel.Info);
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
            DiagnosticMessage($"Attempting to connect to scan heads with timeout of {timeOut} seconds.",LogLevel.Info);
            var disconnectedHeads = scanSystem!.Connect(timeOut);
            if (disconnectedHeads == null)
            {
                throw new InvalidOperationException("ScanSystem.Connect() call timed out.");
            }
            if (disconnectedHeads.Any())
            {
                foreach (var scanHead in disconnectedHeads)
                {
                    DiagnosticMessage($"Scan head {scanHead.SerialNumber} did not respond to connection request.",LogLevel.Error);
                }
                throw new InvalidOperationException($"Not starting scan thread due to connection error.");
            }
            DiagnosticMessage($"All scan heads connected.", LogLevel.Info);
            foreach (var scanHead in scanSystem.ScanHeads)
            {
                DiagnosticMessage($"Active scan head: {scanHead.SerialNumber} ({scanHead.ID}) ",LogLevel.Info);
            }

            DiagnosticMessage($"Using DataFormat {Config.DataFormat}.",LogLevel.Info);
            var minScanPeriod = scanSystem!.GetMinScanPeriod();
            DiagnosticMessage($"ScanSystem reported a MinScanPeriod of {minScanPeriod} μs",LogLevel.Info);
            DiagnosticMessage($"Configuration requested ScanPeriod is {Config.ScanPeriodUs} μs",LogLevel.Info);
            if (Config.ScanPeriodUs < minScanPeriod)
            {
                DiagnosticMessage($"Configuration requested ScanPeriod {Config.ScanPeriodUs} μs is below the system MinScanPeriod {minScanPeriod} μs"+
                                  " - using system MinScanPeriod.",LogLevel.Warn);
            }
            if (PinchotProfileConverter.IsFakeEncoder)
            {
                DiagnosticMessage($"Using fake encoder!",LogLevel.Warn);
            }
            scanSystem.StartScanning(Math.Max(minScanPeriod,Config.ScanPeriodUs), Config.DataFormat, ScanningMode.Frame);
            int failedToPost = 0;
            // we seem to have connected and are scanning
            autoResetEvent.Set();
            while (!ct.IsCancellationRequested)
            {
                // post all profiles that are due
                ct.ThrowIfCancellationRequested();
                
                    var gotProfile = scanSystem.TryTakeFrame(out var frame, TimeSpan.FromMilliseconds(3), ct);
                    if (gotProfile)
                    {
                        //TODO: check the result of Post to see if we lose profiles
                        // due to the downstream processing being too slow
                        if (frame.IsComplete)
                        {
                            for (int i = 0; i < frame.Count; i++)
                            {
                                var postSuccessful = AvailableProfiles.Post(frame[i].ToLogScannerProfile( scanSystem.Units));
                                if (!postSuccessful)
                                {
                                    failedToPost++;
                                    if (failedToPost >= 100)
                                    {
                                        string msg = "BufferBlock failed to post new profiles 100 times.";
                                        DiagnosticMessage(msg, LogLevel.Error);
                                        throw new InternalBufferOverflowException(msg);
                                    }
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
            DiagnosticMessage($"Encountered scanning error: {e}", LogLevel.Error);
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
        var jsSetupFile = Path.Combine(configLocator.GetDefaultConfigLocation(), Config.ScanSystemDefinition);
        DiagnosticMessage($"Configuration uses Definition file {Config.ScanSystemDefinition}.",LogLevel.Info);
        try
        {
            var scanSystem = ScanSystemExtensions.CreateFromFile(jsSetupFile);
            DiagnosticMessage($"ScanSystem created.",LogLevel.Info);
            return scanSystem;
        }
        catch (Exception e)
        {
            DiagnosticMessage($"Failed to create ScanSystem: {e.Message}",LogLevel.Error);
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
