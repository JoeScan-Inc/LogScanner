using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.Pinchot;
using System.Threading.Tasks.Dataflow;
using Point2D = JoeScan.LogScanner.Core.Geometry.Point2D;
using Profile = JoeScan.LogScanner.Core.Models.Profile;

namespace JoeScan.LogScanner.SyntheticDataAdapter;

public class SyntheticDataAdapter : IScannerAdapter
{
    #region Private Fields

    private CancellationTokenSource? cts;
    private Thread? thread;

    #endregion

    #region IScannerAdapter Implementation

    public UnitSystem Units => UnitSystem.Millimeters;
    public BufferBlock<Profile> AvailableProfiles { get; } = new BufferBlock<Profile>(new DataflowBlockOptions
    {
        // unlimited capacity, otherwise the BufferBlock will decline profiles
        // when subsequent steps can't keep up
        BoundedCapacity = -1
    });
    public bool IsRunning { get; private set; }
    public void Configure()
    {
        IsConfigured = true;
    }

    public bool IsReplay => true;
    public bool IsConfigured { get; private set; }
    public void Start()
    {
       
        if (!IsRunning)
        {
            cts = new CancellationTokenSource();
            thread = new Thread(() => ThreadMain(cts.Token)) { IsBackground = true };
            thread.Start();
        }
    }

    public Task StartAsync()
    {
        return Task.Run(Start);
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

    public Task StopAsync()
    {
        return Task.Run(Stop);
    }

    public string Name => "Synthetic Data";
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;
    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;

    #endregion

    #region Event Invocation

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

    protected virtual void OnEncoderUpdated(EncoderUpdateArgs e)
    {
        EncoderUpdated?.Invoke(this, e);
    }

    private void ThreadMain(CancellationToken ct)
    {
        try
        {
            var dg = new DataGenerator(6);
            IsRunning = true;
            OnScanningStarted();
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var profile in dg.NextBatch())
                {
                    AvailableProfiles.Post(profile);
                }
                Thread.Sleep(1);
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
            OnScanningStopped();
        }


    }


    #endregion
}

public class DataGenerator
{
    private readonly uint cycleTimeMs;
    private long currentEncVal = 152000L;
    public record struct HeadCamPair(uint head, uint cam);

    private ulong currentTimeStampNs = 0L;
    private Random random = new Random();

    private List<HeadCamPair> heads = new List<HeadCamPair>()
    {
        new HeadCamPair(0, 0),
        new HeadCamPair(1, 0),
        new HeadCamPair(2, 0),
        new HeadCamPair(0, 1),
        new HeadCamPair(1, 1),
        new HeadCamPair(2, 1),
    };

    private Dictionary<HeadCamPair, uint> seqNum;
    public DataGenerator(uint cycleTimeMs)
    {
        this.cycleTimeMs = cycleTimeMs;
        seqNum = heads.ToDictionary(q => q, l => 0U);

    }

    public IEnumerable<Profile> NextBatch()
    {
        List<Profile> l = new List<Profile>();
        // create six profiles (0A,1A,2A,0B,1B,2B)
        foreach (var pair in heads)
        {
            var p = new Profile();
            p.Camera = pair.cam;
            p.ScanHeadId = pair.head;
            p.LaserIndex = 0;
            p.Inputs = InputFlags.None;
            p.LaserIndex = 1;
            p.SequenceNumber = seqNum[pair]++;
            p.LaserOnTimeUs = 200;
            // p.Data = Array.Empty<Core.Geometry.Point2D>();
            p.Data = RandomNoise(50);
            p.TimeStampNs = currentTimeStampNs++;
            p.EncoderValues = new Dictionary<uint, long>() { { 0, currentEncVal } };
            l.Add(p);
            currentEncVal += 64;
        }
        return l;
    }

    private Point2D[] RandomNoise(int maxPts)
    {
        Point2D[] noise = new Point2D[maxPts];
        for (int i = 0; i < maxPts; i++)
        {
            noise[i] = new Point2D(random.NextDouble() * 600.0 - 300, random.NextDouble() * 400, 0.1);
        }
        return noise;
    }
}
