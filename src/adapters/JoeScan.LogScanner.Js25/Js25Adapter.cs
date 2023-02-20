using Config.Net;
using JoeScan.JCamNet5;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Js25.Enums;
using JoeScan.LogScanner.Js25.Interfaces;
using NLog;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
// we usw the same names as in the JCamNet devkit, so we need to disambiguate here
// the Core.Models enums are designed to be bitwise identical so for JS-25 we can just cast
// to the new type
using InputFlags = JoeScan.LogScanner.Core.Models.InputFlags;
using Profile = JoeScan.LogScanner.Core.Models.Profile;
using ScanFlags = JoeScan.LogScanner.Core.Models.ScanFlags;

namespace JoeScan.LogScanner.Js25;

/// <summary>
/// This class implements the IScannerAdapter interface and provides finished profiles
/// to the LogScanner engine via the AvailableProfiles collection. The collection is filled with
/// Profiles obtained from JS-20/25 scanners that are ready to be assembled. 
/// </summary>
public class Js25Adapter : IScannerAdapter
{
    #region Private Fields

    private Thread workerThread;
    private CancellationTokenSource cancellationTokenSource;
    private Dictionary<uint, ulong> lastTimeInHead = new Dictionary<uint, ulong>();
    private long lastEncoderUpdatePos;
    private int maxRequestedProfileCount;
    private List<IProfileProvider> scanners;
    private int encoderUpdateIncrement;
    private IPAddress baseAddress = IPAddress.None;
    private List<short> cableIdList = new List<short>();
    private string paramFile;
    private SyncMode syncMode = SyncMode.PulseSyncMode;
    private int pulseMasterId;
    private readonly string pluginBaseDir;

    #endregion

    #region Injected Properties

    public IJs25AdapterConfig Config { get; }
    public ILogger Logger { get; }

    #endregion

    #region Lifecycle

    public Js25Adapter(ILogger logger = null)
    {
        AvailableProfiles =
            new BufferBlock<Profile>(new DataflowBlockOptions() { BoundedCapacity = -1, EnsureOrdered = true });
        // since the plugin is loaded into the main application, we need to
        // find the plugin location first
        pluginBaseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        Config = new ConfigurationBuilder<IJs25AdapterConfig>()
            .UseJsonFile(Path.Combine(pluginBaseDir, "Js25Adapter.json"))
            .Build();
        // get injected logger if there is one
        Logger = logger ?? LogManager.GetCurrentClassLogger();
        IsRunning = false;
    }
    #endregion

    #region IScannerAdapter implementation

    public UnitSystem Units { get; }
    public BufferBlock<Profile> AvailableProfiles { get; private set; }
    public bool IsRunning { get; private set; }

    public bool IsReplay => false;
    public uint VersionMajor => 1;
    public uint VersionMinor => 0;
    public uint VersionPatch => 0;
    public Guid Id { get; } = Guid.Parse("{6EAE7379-E27D-4BFE-B304-CF16D40E9A9B}");
    public bool IsConfigured { get; private set; }


    public string Name => "JS-20/JS-25 Single Zone";

    public Task<bool> ConfigureAsync()
    {
        SendInfo($"Configuring adapter {Name}");
        return Task.Run(() => {
            try
            {
                // InternalProfileQueueLength
                var internalProfileQueueLength = Config.InternalProfileQueueLength;
                SendDebug($"InternalProfileQueueLength: {internalProfileQueueLength}");

                //EncoderUpdateIncrement
                encoderUpdateIncrement = Config.EncoderUpdateIncrement;
                SendDebug($"EncoderUpdateIncrement: {encoderUpdateIncrement}");
                //MaxRequestedProfileCount
                maxRequestedProfileCount = Config.MaxRequestedProfileCount;
                SendDebug($"MaxRequestedProfileCount: {maxRequestedProfileCount}");
                // BaseAddress
                string baseAddressString = String.Empty;
                try
                {
                    baseAddressString = Config.BaseAddress;
                    baseAddress = IPAddress.Parse(baseAddressString);
                    SendInfo($"BaseAddress: {baseAddress}");
                }
                catch (Exception e) when (e is FormatException || e is ArgumentNullException)
                {
                    var msg = $"Could not parse BaseAddress: \"{baseAddressString}\".";
                    SendError(e, msg);
                    throw new ApplicationException(msg);
                }

                // CableIdList
                string idsString = Config.CableIdList;
                string[] idStrings = idsString.Split(new[] { ',' });
                try
                {
                    cableIdList = idStrings.Select(Int16.Parse).ToList();
                    SendInfo($"CableIDList: {String.Join(",", cableIdList)}");
                }
                catch (Exception e)
                {
                    var msg = $"Could not parse CableIdList: \"{idsString}\".";
                    SendError(e, msg);
                    throw new ApplicationException(msg);
                }

                // ParamFile
                paramFile = Config.ParamFile;
                if (String.IsNullOrEmpty(paramFile))
                {
                    var msg = $"Could not parse ParamFile: \"{paramFile}\".";
                    SendError(msg);
                    throw new ApplicationException(msg);
                }

                // combine handles absolute paths in the second argument by returning just that
                string tmpPath = Path.Combine(pluginBaseDir, paramFile);
                if (File.Exists(tmpPath))
                {
                    paramFile = tmpPath;
                    SendInfo($"ParamFile: {paramFile}");
                }
                else
                {
                    var msg = $"Could not find ParamFile: \"{paramFile}\".";
                    SendError(msg);
                    throw new ApplicationException(msg);
                }

                syncMode = Config.SyncMode;
                SendDebug($"SyncMode: {syncMode}");


                if (syncMode == SyncMode.PulseSyncMode)
                {
                    pulseMasterId = Config.PulseMasterId;
                    SendDebug($"PulseMasterId: {pulseMasterId}");
                }

                IsConfigured = true;
                return true;
            }
            catch (Exception e)
            {
                SendError($"Configuration of adapter failed with error {e.Message}");
                IsConfigured = false;
                return false;
            }
        });
        
    }

    public Task<bool> StartAsync()
    {
        return Task.Run((() =>
        {
            Start();
            return IsRunning;
        }));
    }
    //TODO: this is just a place holder until we can get 
    // proper StartAsync in place
    public void Start()
    {
        SendInfo("Attempting to start.");
        if (IsRunning)
        {
            SendWarning("Failed because IsRunning is already true.");
            return;
        }
        if (!IsConfigured)
        {
            var msg = $"Adapter {Name} needs to be configured first.";
            SendError(msg);
            throw new ApplicationException(msg);
        }
        StartScanThread();
    }

    public void Stop()
    {
        SendInfo("Stop()");
        if (workerThread != null && IsRunning && cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel(true);
            if (!workerThread.Join(100))
            {
                workerThread = null;
                throw (new ApplicationException("Scanning Thread failed to stop"));
            }
        }
    }

    #endregion

    #region Adapter Feedback Messages

    private void SendInfo(string message)
    {
        OnAdapterMessage(LogLevel.Info, message);
        Logger.Info(message);
    }

    private void SendDebug(string message)
    {
        OnAdapterMessage(LogLevel.Debug, message);
        Logger.Debug(message);
    }


    private void SendWarning(string message)
    {
        OnAdapterMessage(LogLevel.Warn, message);
        Logger.Warn(message);
    }

    private void SendError(string message)
    {
        OnAdapterMessage(LogLevel.Error, message);
        Logger.Error(message);
    }
    private void SendError(Exception e,string msg)
    {
        OnAdapterMessage(LogLevel.Error, $"{msg} ({e.Message})");
        Logger.Error(e,msg);
    }

    #endregion

    #region Events

    public event EventHandler ScanningStarted;
    public event EventHandler ScanningStopped;
    public event EventHandler ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs> EncoderUpdated;
    public event EventHandler<AdapterMessageEventArgs>? AdapterMessage;

    #endregion

    #region Event Invocations

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

    protected  void OnAdapterMessage(AdapterMessageEventArgs e)
    {
        AdapterMessage?.Invoke(this, e);
    }

    protected void OnAdapterMessage(NLog.LogLevel lvl, string msg)
    {
        OnAdapterMessage(new AdapterMessageEventArgs(lvl,msg));
    }
    #endregion

    #region Scan Thread

    private void ThreadMain(CancellationToken cancellationToken)
    {
        List<Profile> receivedProfiles = new List<Profile>();
        // Main loop enters encoder sync mode then starts collection thread
        try
        {
            IsRunning = true;
            lastEncoderUpdatePos = 0;
            OnScanningStarted(EventArgs.Empty);
            scanners = new List<IProfileProvider>(ScannerFactory.Connect(baseAddress, cableIdList.ToArray()));
            if (scanners.Count != cableIdList.Count)
            {
                // not all heads connected, bail
                var unconnectedIds = cableIdList.Except(scanners.Select(q => q.CableID));
                var msg = $"Failed to connect to cable ids {String.Join(',', unconnectedIds)}";
                SendError(msg);
                throw new ApplicationException(msg);
            }
            switch (syncMode)
            {
                case SyncMode.EncoderSyncMode:
                    EnterEncoderSyncMode();
                    break;
                case SyncMode.PulseSyncMode:
                    EnterPulseSyncMode();
                    //TODO: rename to make clear it is ms or us 
                    StartPulseMaster(Config.PulseInterval);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //re-initialize the lastTimeInHead dictionary
            lastTimeInHead.Clear();
            foreach (var id in scanners.Select(q=>q.CableID))
            {
                lastTimeInHead[(uint)id] = 0;
            }

            // pre-request scans from all scanners                    
            PrimeReadoutLoop();

            for (; ; )
            {
                // collection loop                    
                // the scannerConnection may throw anywhere in the parallel execution of the 
                // communication with the hardware. If done in parallel, it will be returned as 
                // an AggregateException, if not, a ScannerCommunicationException
                GetProfilesParallel(receivedProfiles, maxRequestedProfileCount);
                if (receivedProfiles.Any())
                {
                    // when in SyncMode, but with no triggers for more than a few seconds
                    // the first profile is always noisy and should be thrown out
                    // int removed = receivedProfiles.RemoveAll(
                    //     profile =>
                    //         lastTimeInHead[profile.ScanHeadId] > 0 &&
                    //         lastTimeInHead[profile.ScanHeadId] - profile.TimeStampNs > 5E9);
                    // if (removed > 0)
                    // {
                    //     SendInfo("Removed {0} possibly corrupted profiles from queue.", removed);
                    // }

                    foreach (var profile in receivedProfiles)
                    {
                        lastTimeInHead[profile.ScanHeadId] = profile.TimeStampNs;
                    }

                    foreach (var receivedProfile in receivedProfiles)
                    {
                        //TODO: enable
                        AvailableProfiles.Post(receivedProfile);
                    }

                    UpdateEncoderPosition(receivedProfiles);
                    receivedProfiles.Clear();
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

        }
        catch (OperationCanceledException)
        {
            // not an error condition
            ExitSyncMode();
        }
        catch (AggregateException e)
        {
            foreach (var innerException in e.Flatten().InnerExceptions)
            {
                SendError(innerException, "Scanner Communication Problem:");
            }
            OnScanErrorEncountered(EventArgs.Empty);
        }
        catch (Exception e)
        {
            SendError(e, "Scanner Communication Problem:");
            OnScanErrorEncountered(EventArgs.Empty);
        }
        finally
        {
            IsRunning = false;
        }
        OnScanningStopped(EventArgs.Empty);
        cancellationTokenSource = null;
    }


    #endregion

    #region New region

    private void StartScanThread()
    {
        SendInfo("StartScanThread()");
        
        if (workerThread == null)
        {
            // we can't keep a token source around, because once it's been "canceled",
            // there is no way to "reset" it, so the next StartScanning would fall right through 
            cancellationTokenSource = new CancellationTokenSource();
            workerThread = new Thread(() => ThreadMain(cancellationTokenSource.Token))
            {
                IsBackground = true,
                Name = "Scanning Thread",
                Priority = ThreadPriority.Highest
            };
            workerThread.Start();
        }
    }

    public void SendParameters(String parameters)
    {
        foreach (IProfileProvider s in scanners)
        {
            s.SetParameters(parameters, true);
        }
    }

    private void EnterEncoderSyncMode()
    {
        Parallel.ForEach(scanners, s => s.EnterEncoderSyncMode());
    }


    private void EnterPulseSyncMode()
    {
        Parallel.ForEach(scanners, s => s.EnterPulseSyncMode());
    }


    private void ExitSyncMode()
    {
        Parallel.ForEach(scanners, s => s.ExitSyncMode());
    }

    private void PrimeReadoutLoop()
    {
        Parallel.ForEach(scanners, s => s.BeginGetQueuedProfiles(1));
    }

    private void GetProfilesParallel(List<Profile> receivedProfiles, int maxProfilesRequested = 1)
    {
        Parallel.ForEach(scanners, s => s.BeginGetQueuedProfiles(maxProfilesRequested));
        object locker = new object();
        Parallel.ForEach(scanners, s =>
        {
            // transform to unified (LogScanner specific) profile and stash in outgoing list
            var p = s.EndGetQueuedProfiles().Select(p => Convert(p, s.CableID));
            //TODO: transform and add to outgoing
            lock (locker)
            {
                receivedProfiles.AddRange(p);
            }
        });
    }

    private Profile Convert(JCamNet5.Profile p, short cableId)
    {
        //TODO: set the Unit based on param.dat
        var np = new Profile();
        {
            np.Data = p.Select(q => new Point2D(q.X, q.Y, q.Brightness)).ToArray();
        };

        np.Camera = 1;
        np.EncoderValues[0] = p.Location;
        np.ScanningFlags = (ScanFlags)((int)p.Flags); // we get away with this because the first 3 elements in
                                                      // the enum for the new scan flags are the same
        np.LaserIndex = 1;
        np.LaserOnTimeUs = (ushort)p.LaserOnTime; //TODO: verify range
        np.SequenceNumber = (uint)p.SequenceNumber; //TODO: verify range and handle rollover
        np.TimeStampNs = (ulong)(p.TimeInHead * 1E6);
        np.ScanHeadId = (uint)cableId;
        np.Inputs = (InputFlags)((int)np.Inputs);

        return np;

    }

    private void StartPulseMaster(int pulseInterval)
    {
        scanners[0].StartPulseMaster(pulseInterval, 0);
    }
    private void UpdateEncoderPosition(IEnumerable<Profile> receivedProfiles)
    {
        var p = receivedProfiles.FirstOrDefault(s => s.ScanHeadId == 0);
        if (p != null && p.EncoderValues.ContainsKey(0))
        {
            if (lastEncoderUpdatePos == 0)
            {
                lastEncoderUpdatePos = p.EncoderValues[0];
                return;
            }
            if (p.EncoderValues[0] > lastEncoderUpdatePos + encoderUpdateIncrement)
            {
                lastEncoderUpdatePos = p.EncoderValues[0];
                // TODO: fix with new EncoderUpdateArgs
                //  OnEncoderUpdated(new EncoderUpdateArgs(lastEncoderUpdatePos, p.TimeStampNs));
            }
        }
    }
    #endregion

    
}
