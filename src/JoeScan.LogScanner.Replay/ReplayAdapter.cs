using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Helpers;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using Nini.Config;
using NLog;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Replay;

public class ReplayAdapter : IScannerAdapter
{
    private CancellationTokenSource? cts;
    private Thread? thread;
    
    // simple test adapter that replays profiles from a file for testing 
    // purposes

    // we have test data from an install in mm
    public UnitSystem Units => UnitSystem.Millimeters;
    
    public BufferBlock<Profile> AvailableProfiles { get; } =
        new BufferBlock<Profile>(new DataflowBlockOptions
        {
            BoundedCapacity = 1

        });

    public List<Tuple<int, int>> SequenceList { get; private set; } = new List<Tuple<int, int>>();// this is a list of indexes 
    // into the LegacyProfiles list, sorted by time. The idea is that we run a timer, and post the profiles
    // when their time has come

    public bool IsRunning { get; private set; }
    public void Configure()
    {
        // nothing to do here
    }

    public bool IsConfigured => true;
    public IConfigSource Config { get; set; }
    public ILogger Logger { get; set; }
    private List<LegacyProfile> LegacyProfiles { get; set; } = new();


    public ReplayAdapter([KeyFilter("ReplayAdapter.ini")] IConfigSource config, ILogger? logger = null)
    {
        Config = config;
        // get injected logger if there is one
        Logger = logger ?? LogManager.GetCurrentClassLogger();
        IsRunning = false;
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

    private void ThreadMain(CancellationToken ct)
    {
        FillBuffer();  
        try
        {
            IsRunning = true;
            var sw = Stopwatch.StartNew();
            var index = 0;
           
            var gapTimeCount = 0;
            while (!ct.IsCancellationRequested && index < SequenceList.Count)
            {
                // post all profiles that are due
                while (sw.ElapsedMilliseconds > SequenceList[index].Item1)
                {
                    ct.ThrowIfCancellationRequested();
                   
                    AvailableProfiles.Post(LegacyProfiles[SequenceList[index++].Item2].Convert());
                    if (index >= SequenceList.Count) 
                        break;
                    gapTimeCount = 0;
                }
                Thread.Sleep(1); 
                // problem is that the raw test data has gaps 
            // in between logs, where no profiles were recorded, 
            // which holds up processing until the next log starts
            // to combat that we insert empty profiles if there is a time gap of 
            // more than a few ms
                gapTimeCount++;
                if (gapTimeCount > 100)
                {
                    for(int i=0; i < 20; i++)
                    {
                        AvailableProfiles.Post(LegacyProfile.CreateEmpty().Convert());
                    }
                    gapTimeCount = 0;
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

    private async void PostToBuffer()
    {
        IsRunning = true;
        try
        {
            foreach (var legacyProfile in LegacyProfiles)
            {
                await AvailableProfiles.SendAsync(legacyProfile.Convert()).ConfigureAwait(false);
                await Task.Delay(1).ConfigureAwait(false);

            }
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void FillBuffer()
    {
        string simFile;
        try
        {
            simFile = Config.Configs["Replay"].GetString("File");
        }
        catch (Exception e)
        {
            Logger.Fatal(e, "Failed to get replay file name from configuration. ");
            throw;
        }

        try
        {
            using var fs = new FileStream(simFile, FileMode.Open);
            using var br = new BinaryReader(fs);

            while (true)
            {
                LegacyProfiles.Add(ReadFromBinaryReader(br));
            }
        }
        catch (EndOfStreamException )
        {
            //ignore, eof
        } 
        catch (Exception e)
        {
            // anything else
            Logger.Fatal(e, $"Failed to open/read replay file: {simFile}");
            throw;
        }

        BuildTimingIndex(LegacyProfiles);
    }

    private void BuildTimingIndex(List<LegacyProfile> legacyProfiles)
    {
        var startValue = new Dictionary<int, int>();
        var sequenceList = new List<Tuple<int, int>>(legacyProfiles.Count);
        foreach (var (profile,index) in legacyProfiles.WithIndex())
        {
            if (!startValue.ContainsKey(profile.CableId))
            {
                startValue[profile.CableId] = profile.SendLocation; // could also use time in head
            }

            var timeDiff = profile.SendLocation - startValue[profile.CableId];
            if (timeDiff >= 0)
            {
                sequenceList.Add(new Tuple<int, int>(timeDiff, index));
            }
        }

        SequenceList = sequenceList.OrderBy(q => q.Item1).ToList();
    }

  

    public Task StartAsync()
    {
        return Task.Run(Start);
    }

    public void Stop()
    {

    }

    public Task StopAsync()
    {
        return Task.Run(Stop);
    }

    public string Name => "Replay";

    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;

    // embedded helper class to read the binary format of some old logscanner implementations
    public static LegacyProfile ReadFromBinaryReader(BinaryReader br, bool ignoreInputs = false)
    {
        var sp = new LegacyProfile();
        sp.CableId = br.ReadInt32();
        sp.ScanningFlags = (ScanFlags)br.ReadInt32();
        sp.LaserIndex = br.ReadInt32();
        sp.LaserOnTime = br.ReadInt32();
        sp.Location = br.ReadInt32();
        sp.SendLocation = br.ReadInt32();
        sp.SequenceNumber = br.ReadInt32();
        sp.TimeInHead = br.ReadInt32();
        sp.Z = br.ReadDouble();
        if (!ignoreInputs)
        {
            // older versions had a bug and didn't save the inputs flag into the profile
            sp.Inputs = (InputFlags)br.ReadInt32();
        }
        int dataLength = br.ReadInt32();
        var l = new List<Point2D>(dataLength);
        for (int i = 0; i < dataLength; i++)
        {
            var x = br.ReadInt16() / 10.0;
            var y = br.ReadInt16() / 10.0;
            var b = br.ReadInt16() / (short.MaxValue - 1);
            l.Add(new Point2D() { X = x, Y = y, B = b });
        }
        sp.Data = l.ToArray();
        return sp;
    }

    #region Internal Helper

    [DebuggerDisplay("Id = {CableId}, NumPoints = {Data.Length}, Z={Z}")]
    public class LegacyProfile
    {
        public int CableId { get; set; }
        public ScanFlags ScanningFlags { get; set; }
        public int LaserIndex { get; set; }
        public int LaserOnTime { get; set; }
        public int Location { get; set; }
        public int SendLocation { get; set; }
        public int SequenceNumber { get; set; }
        public int TimeInHead { get; set; }
        public double Z { get; set; }
        public InputFlags Inputs { get; set; }
        public Point2D[]? Data { get; set; } 
        public Profile Convert()
        {
            var np = new Profile
            {
                Data = new Point2D[Data.Length],
                ScanHeadId = (uint)CableId,
                Camera = 1,
                EncoderValues =
                    {
                        [0] = Location
                    },
                ScanningFlags = (ScanFlags)((int)ScanningFlags), // we get away with this because the first 3 elements in
                                                                 // the enum for the new scan flags are the same
                LaserIndex = 1,
                LaserOnTimeUs = (ushort)LaserOnTime, //TODO: verify range
                SequenceNumber = (uint)SequenceNumber, //TODO: verify range and handle rollover
                TimeStampNs = (ulong)(TimeInHead * 1E6),
                Inputs = (InputFlags)((int)Inputs)
            };
            Array.Copy(Data, np.Data, np.Data.Length);

            return np;

        }

        public static LegacyProfile CreateEmpty()
        {
            return new LegacyProfile() { CableId = 0, Data = Array.Empty<Point2D>() };
        }

    }

    #endregion
}
