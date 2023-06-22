using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Enums;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Models;

public class SingleZoneLogAssembler : ILogAssembler
{
    private readonly IEnumerable<ILogStatusEventConsumer> statusEventConsumers;
    private readonly double encoderPulseInterval;
    private readonly bool useLogPresenceSignal;
    private readonly bool startScanInverted;
    private readonly int startLogCount;
    private readonly int stopLogCount;
    private readonly double minLogLength;
    private readonly double maxLogLength;
    private readonly double minProfileSpacing;

    #region Lifecycle

    public SingleZoneLogAssembler(
        IRawProfileValidator profileValidator,
        IPieceNumberProvider numerator,
        IEnumerable<ILogStatusEventConsumer> statusEventConsumers,
        ILogger logger,
        SingleZoneLogAssemblerConfig config,
        CoreConfig coreConfig)
    {
        this.statusEventConsumers = statusEventConsumers;
        Config = config;
        ProfileValidator = profileValidator;
        Numerator = numerator;
        Logger = logger;

        Logger.Debug("Created SingleZoneLogAssembler");

        accumulatedProfiles = new LinkedList<Profile>();
        SetCurrentState(LogAssemblerState.Idle);
      
        // we initialize these fields here instead of reading from the config 
        // where they are used, because Config.Net has no caching and will re-read the JSON 
        // on every access
        encoderPulseInterval = coreConfig.EncoderPulseInterval;
        useLogPresenceSignal = Config.UseLogPresenceSignal;
        startScanInverted = Config.StartScanInverted;
        startLogCount = Config.StartLogCount;
        stopLogCount = Config.StopLogCount;
        minLogLength = Config.MinLogLength;
        maxLogLength = Config.MaxLogLength;
        minProfileSpacing = Config.MinProfileSpacing;
    }

    #endregion

    #region ILogAssembler Implementation

    public BufferBlock<RawLog> RawLogs { get; } =
        new BufferBlock<RawLog>(new DataflowBlockOptions
        {
            BoundedCapacity = 1,

        });

    #endregion

    public void AddProfile(Profile p)
    {
        bool isValidProfile;
        if (useLogPresenceSignal)
        {
            isValidProfile = ProfileValidator.IsValid(p) && startScanInverted
                ? !p.Inputs.HasFlag(InputFlags.StartScan)
                : p.Inputs.HasFlag(InputFlags.StartScan);
        }
        else
        {
            isValidProfile = ProfileValidator.IsValid(p);
        }

        switch (GetCurrentState())
        {
            case LogAssemblerState.Idle:
                if (isValidProfile)
                {
                    accumulatedProfiles.AddLast(p);
                    SetCurrentState(LogAssemblerState.Tentative);
                }

                break;
            case LogAssemblerState.Tentative:
                if (isValidProfile)
                {
                    accumulatedProfiles.AddLast(p);
                }
                else
                {
                    SetCurrentState(LogAssemblerState.Idle);
                    break;
                }

                if (accumulatedProfiles.Count > startLogCount)
                {
                    SetCurrentState(LogAssemblerState.Collecting);
                }

                break;
            case LogAssemblerState.Collecting:
                bool stopped;
                bool reversed;
                IsStoppedOrReversed(p, out stopped, out reversed);

                if (reversed && isValidProfile)
                {
                    Logger.Trace("reversed scan direction detected. Removing already collected profiles.");
                    RemoveFromEnd(p);
                    if (accumulatedProfiles.Count == 0)
                    {
                        SetCurrentState(LogAssemblerState.Idle);
                    }

                    break;
                }

                if (stopped)
                {
                    // do nothing, just go to the next profile
                    break;
                }

                if (!isValidProfile)
                {
                    consecutiveNoLogProfiles++;
                }
                else
                {
                    accumulatedProfiles.AddLast(p);
                    // reset the counter, we got a live one
                    consecutiveNoLogProfiles = 0;
                }

                double scannedSoFar = EstimateScannedLength();
                // TODO: send slices for all newly collected profiles here so we have 
                // a chance to calculate on the fly

                // check how many consecutive empty profiles we had so far while collecting
                if (consecutiveNoLogProfiles > stopLogCount )
                {
                    // looks like we have reached the end of a log, or the end of a piece of debris
                    if (scannedSoFar > minLogLength)
                    {
                        // we  have a bona fide log
                        // signal and break
                        foreach (var logStatusEventConsumer in statusEventConsumers)
                        {
                            Task.Run(() => logStatusEventConsumer.LogCollectionAborted()).Forget();
                        }
                        LogReady();
                    }

                    // fall through for debris, this will clean up:
                    SetCurrentState(LogAssemblerState.Idle);
                    break;
                }

                if (scannedSoFar >= maxLogLength)
                {
                    foreach (var logStatusEventConsumer in statusEventConsumers)
                    {
                        Task.Run(() => logStatusEventConsumer.LogCollectionAborted()).Forget();
                    }
                    LogReady();
                    SetCurrentState(LogAssemblerState.Idle);
                }

                break;
        }
    }

    internal void IsStoppedOrReversed(Profile profile, out bool stopped, out bool reversed)
    {
        stopped = false;
        reversed = false;
        var last = accumulatedProfiles.Last;
        do
        {
            if (last == null)
            {
                break;
            }

            if (last.Value.ScanHeadId == profile.ScanHeadId && last.Value.CameraIndex == profile.CameraIndex)
            {
                reversed = profile.Encoder - last.Value.Encoder < 0;
                
                stopped = Math.Abs((profile.Encoder * encoderPulseInterval)
                                   - (last.Value.Encoder * encoderPulseInterval)) <
                          minProfileSpacing;
                break;
            }

            last = last.Previous;
        } while (last != accumulatedProfiles.First);
    }

    internal void RemoveFromEnd(Profile profile)
    {
        // go through the accumulated profiles from the end, and remove everything that has
        // the same cable id and a z that is lower than this profile
        var last = accumulatedProfiles.Last;
        while (last != null)
        {
            if (last.Value.ScanHeadId == profile.ScanHeadId &&
                last.Value.Encoder * encoderPulseInterval >
                profile.Encoder * encoderPulseInterval)
            {
                var temp = last.Previous;
                accumulatedProfiles.Remove(last);
                last = temp;
            }
            else
            {
                last = last.Previous;
            }
        }
    }

    internal double EstimateScannedLength()
    {
        if (accumulatedProfiles.Count <= 1)
        {
            return 0.0;
        }

        //TODO: we may want to only use a single head (top) for length estimates
        var last = accumulatedProfiles.LastOrDefault(n => n.ScanHeadId == accumulatedProfiles.First().ScanHeadId);
        if (last != null)
        {
            //TODO: does travel direction matter?
            return (last.Encoder - accumulatedProfiles.First().Encoder) * encoderPulseInterval;
        }

        return 0.0;
    }

    private async void LogReady()
    {
        Stopwatch sw = Stopwatch.StartNew();
        RawLog rawLog = new RawLog(Numerator.GetNextPieceNumber(), accumulatedProfiles);
        sw.Stop();
        Logger.Trace($"Sorting raw profiles took {sw.ElapsedMilliseconds} ms");
        await RawLogs.SendAsync(rawLog).ConfigureAwait(false);
    }
    #region Injected Properties

    public SingleZoneLogAssemblerConfig Config { get; }
    public IRawProfileValidator ProfileValidator { get; }
    public IPieceNumberProvider Numerator { get; }
    public ILogger Logger { get; }

    #endregion

    #region Fields

    protected LinkedList<Profile> accumulatedProfiles;
    private LogAssemblerState currentState;
    private int consecutiveNoLogProfiles;

    #endregion

    #region Properties

    protected LogAssemblerState GetCurrentState()
    {
        return currentState;
    }

    protected void SetCurrentState(LogAssemblerState newState)
    {
        if (newState != currentState)
        {
            // we switched from Tentative or Collecting back to Idle
            // and throw away all accumulated profiles
            Logger.Trace($"Switching from {currentState} to {newState}");
            if (newState == LogAssemblerState.Idle)
            {
                Logger.Trace($"New state is {newState} - cleaning up accumulated junk profiles.");
                accumulatedProfiles.Clear();
                consecutiveNoLogProfiles = 0;
                // FlagCount = 0;
                foreach (var logStatusEventConsumer in statusEventConsumers)
                {
                    Task.Run(() => logStatusEventConsumer.LogCollectionIdleStarted()).Forget();
                }
            }
            else
            {
                foreach (var logStatusEventConsumer in statusEventConsumers)
                {
                    Task.Run(() => logStatusEventConsumer.LogCollectionIdleEnded()).Forget();
                }
            }

            //TODO: enable state change event
            //  var args = new StateChangeEventArgs(currentState, value, currentEncVal);
            //OnStateChanged(args);
            currentState = newState;
        }
    }

    #endregion
}
