using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Adapters.Replay;

public class ReplayAdapter : AdapterBase, IScannerAdapter
{
    #region Injected Properties

    private IReplayAdapterConfig Config { get; }

    #endregion

    #region Private Fields

    private CancellationTokenSource? cts;
    private Thread? thread;
    private bool isRunning;

    #endregion

    // simple test adapter that replays profiles from a file for testing 
    // purposes

    // we have test data from a JS-25 install in mm

    #region Private Properties

    private List<Tuple<int, int>> SequenceList { get; set; } =
        new List<Tuple<int, int>>(); // this is a list of indexes 
    // into the LegacyProfiles list, sorted by time. The idea is that we run a timer, and post the profiles
    // when their time has come

    private List<Profile> PlaybackProfiles { get; set; } = new();

    #endregion

    #region Lifecycle

    public ReplayAdapter(IReplayAdapterConfig config, ILogger logger)
    :base(logger)
    {
        Config = config;
        // get injected logger if there is one
        
        IsRunning = false;
    }

    #endregion


    #region IScannerAdapter implementation

    public Task<bool> ConfigureAsync()
    {
        return Task.FromResult(true);
    }

    public bool IsConfigured => true;

    public bool IsReplay => true;
    public uint VersionMajor => 1;
    public uint VersionMinor => 0;
    public uint VersionPatch => 0;
    
    public Guid Id { get; } = Guid.Parse("{65FA101D-22EF-4161-8BBB-1E167986A126}");
    public bool IsRunning { get; private set; }


    public BufferBlock<Profile> AvailableProfiles { get; } =
        new BufferBlock<Profile>(new DataflowBlockOptions
        {
            // unlimited capacity, otherwise the BufferBlock will decline profiles
            // when subsequent steps can't keep up
            BoundedCapacity = -1
        });

    public UnitSystem Units => UnitSystem.Inches;

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
            DiagnosticMessage("Starting replay adapter", LogLevel.Info);
            cts = new CancellationTokenSource();
            thread = new Thread(() => ThreadMain(cts.Token)) { IsBackground = true };
            thread.Start();
            OnScanningStarted();
        }
    }


    public void Stop()
    {
        if (cts != null)
        {
            DiagnosticMessage("Stopping replay adapter", LogLevel.Info);
            cts.Cancel();
            thread!.Join();
            OnScanningStopped();
        }
    }

    public string Name => "Replay";

    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;

    #endregion

    
    #region Private Methods

    private void ThreadMain(CancellationToken ct)
    {
        try
        {
            IsRunning = true;
            
            FillBuffer();
            
            while (true)
            {
                DiagnosticMessage($"Replaying {PlaybackProfiles.Count} recorded profiles", LogLevel.Info);
                var index = 0;
                while (index < PlaybackProfiles.Count)
                {
                    ct.ThrowIfCancellationRequested();
                    var profile = PlaybackProfiles[index];
                    // need to clone profiles, as they are not immutable and we don't want to change the original
                    if (!AvailableProfiles.Post((Profile)profile.Clone()))
                    {
                        // unable to post, buffer is full
                        // unlikely, since we set the buffer to unlimited capacity
                        DiagnosticMessage("Dropped profile.", LogLevel.Warn);
                    }


                    index++;
                    Thread.Sleep(1);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // perfectly normal exception we get when using the token to cancel
            DiagnosticMessage("Replay cancelled", LogLevel.Info);
        }
        catch (Exception e)
        {
            DiagnosticMessage($"Replay failed : {e.Message}", LogLevel.Error);
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
            var msg = $"Using replay file: {Config.File}";
            simFile = Config.File;
            
        }
        catch (Exception e)
        {
            DiagnosticMessage($"Failed to get replay file from configuration: {Config.File}", LogLevel.Fatal);
            throw;
        }

        try
        {
            PlaybackProfiles.Clear();
            using var fs = new FileStream(simFile, FileMode.Open);
            using GZipStream zip = new GZipStream(fs, CompressionMode.Decompress, true);
            using var br = new BinaryReader(zip);
            while (true)
            {
                PlaybackProfiles.Add(ProfileReaderWriter.Read(br));
            }
        }
        catch (EndOfStreamException)
        {
            //ignore, eof
        }
        catch (Exception e)
        {
            // anything else
            DiagnosticMessage($"Failed to read replay file: {Config.File}", LogLevel.Fatal);
            throw;
        }

        // debugging code:

        // for (uint headIndex = 0; headIndex < 3; headIndex++)
        // {
        //     var profiles = PlaybackProfiles.Where(q => q.ScanHeadId == headIndex).ToArray();
        //     using (var s = File.OpenWrite($"C:\\Users\\fabian\\synced\\Customer Support\\Mebor\\May2023\\2023_2_27_17_14_27\\{headIndex}.csv"))
        //     using (var sw = new StreamWriter(s))
        //     {
        //         foreach (var p in profiles)
        //         {
        //             var tsMs = p.TimeStampNs - profiles[0].TimeStampNs;
        //
        //             sw.WriteLine($"{p.TimeStampNs},{p.EncoderValues[0]},{p.ScanHeadId},{p.Data.Length}");
        //         }
        //         
        //     }
        // }
    }

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

    
    #endregion
}
