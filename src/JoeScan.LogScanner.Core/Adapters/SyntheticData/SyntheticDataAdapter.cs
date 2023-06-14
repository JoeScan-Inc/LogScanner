using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Profile = JoeScan.LogScanner.Core.Models.Profile;

namespace JoeScan.LogScanner.Core.Adapters.SyntheticData;

public class SyntheticDataAdapter : IScannerAdapter
{
    private readonly FakeLogGenerator fakeLogGenerator;
    private readonly ILogger logger;
    public ISyntheticDataAdapterConfig Config { get; }

    #region Private Fields

    private CancellationTokenSource? cts;
    private Thread? thread;
    private Stopwatch timeBase;
    private readonly double tickFrequency;

    #endregion

    #region Lifecycle

    public SyntheticDataAdapter(ISyntheticDataAdapterConfig config,
        FakeLogGenerator fakeLogGenerator, ILogger logger)
    {
        this.fakeLogGenerator = fakeLogGenerator;
        this.logger = logger;
        Config = config;
        timeBase = new Stopwatch();
        tickFrequency = (double) Stopwatch.Frequency;
        timeBase.Start();
    }

    #endregion

    #region IScannerAdapter Implementation

    public UnitSystem Units => Config.Units;
    public BufferBlock<Profile> AvailableProfiles { get; } = new BufferBlock<Profile>(new DataflowBlockOptions
    {
        // unlimited capacity, otherwise the BufferBlock will decline profiles
        // when subsequent steps can't keep up
        BoundedCapacity = -1
    });

    public bool IsRunning { get; private set; }
    public Task<bool> ConfigureAsync()
    {
        return Task.FromResult(true);
    }

    public bool IsReplay => true;
    public bool IsConfigured { get; private set; }

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
        if (!IsRunning)
        {
            cts = new CancellationTokenSource();
            thread = new Thread(() => ThreadMain(cts.Token)) { IsBackground = true };
            thread.Start();
        }
    }

    public void Stop()
    {
        if (cts != null)
        {
            cts.Cancel();
            thread!.Join();
            OnScanningStopped();
        }
    }

    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;
    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<PluginMessageEventArgs>? PluginMessage;

    #endregion

    #region Event Invocation

    protected virtual void OnScanningStarted()
    {
        ScanningStarted?.Raise(this, EventArgs.Empty);
    }

    protected virtual void OnScanningStopped()
    {
        ScanningStopped?.Raise(this, EventArgs.Empty);
    }

    protected virtual void OnScanErrorEncountered()
    {
        ScanErrorEncountered?.Raise(this, EventArgs.Empty);
    }

    protected virtual void OnEncoderUpdated(EncoderUpdateArgs e)
    {
        EncoderUpdated?.Raise(this, e);
    }

    protected virtual void OnAdapterMessage(PluginMessageEventArgs e)
    {
        PluginMessage?.Invoke(this, e);
    }

    private void ThreadMain(CancellationToken ct)
    {
        var encoderPulseInterval = Config.EncoderPulseInterval; // in Unit, i.e. mm or inch
        var chainSpeed = Config.ChainSpeed; // in Unit/s i.e. mm/s or inch/s
        long lastEncoderUpdateVal = 0L;                                            
        try
        {
            
            IsRunning = true;
            OnScanningStarted();
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var time = timeBase.ElapsedTicks / tickFrequency; // time in seconds
                var distance = chainSpeed * time;
                var encVal = (long)( distance / encoderPulseInterval);
                if (lastEncoderUpdateVal == 0)
                {
                    lastEncoderUpdateVal = encVal;
                }

                if (encVal - lastEncoderUpdateVal > 1000)
                {
                    lastEncoderUpdateVal = encVal;
                    OnEncoderUpdated(new EncoderUpdateArgs(0,0,0,0,0,0,lastEncoderUpdateVal,0));
                }
                // temporarily
                Thread.Sleep(1);
                foreach (var profile in fakeLogGenerator.ProfileForEncoderValue(encVal, (long)(time * 1E09)))
                {
                    AvailableProfiles.Post(profile);
                }

                


            }
        }
        catch (OperationCanceledException)
        {
            // perfectly normal exception we get when using the token to cancel
        }
        catch (Exception)
        {
            // unused
        }
        finally
        {
            IsRunning = false;
        }
    }


    #endregion

    public string Name => "Synthetic Data";
    public uint VersionMajor => 1;
    public uint VersionMinor => 0;
    public uint VersionPatch => 0;
    public Guid Id { get; } =  Guid.Parse("{C79255EF-9AB8-4B6D-B3F1-FA4D37AFD021}");
}
