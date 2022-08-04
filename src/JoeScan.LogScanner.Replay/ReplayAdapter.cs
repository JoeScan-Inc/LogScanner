using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Helpers;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Replay;

public class ReplayAdapter : IScannerAdapter
{
    #region Injected Properties
    private IReplayAdapterConfig Config { get; }
    public ILogger Logger { get; set; }
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

    private List<Tuple<int, int>> SequenceList { get; set; } = new List<Tuple<int, int>>(); // this is a list of indexes 
    // into the LegacyProfiles list, sorted by time. The idea is that we run a timer, and post the profiles
    // when their time has come

    private List<Profile> PlaybackProfiles { get; set; } = new();

    #endregion

    #region Lifecycle

    public ReplayAdapter(IReplayAdapterConfig config, ILogger? logger = null)
    {
        Config = config;
        // get injected logger if there is one
        Logger = logger ?? LogManager.GetCurrentClassLogger();
        IsRunning = false;
    }

    #endregion


    #region IScannerAdapter implementation

    public void Configure()
    {
        // nothing to do here
    }

    public bool IsConfigured => true;

    public bool IsReplay => true;
    public bool IsRunning
    {
        get => isRunning;
        private set
        {
            if (isRunning != value)
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

    public BufferBlock<Profile> AvailableProfiles { get; } =
        new BufferBlock<Profile>(new DataflowBlockOptions
        {
            // unlimited capacity, otherwise the BufferBlock will decline profiles
            // when subsequent steps can't keep up
            BoundedCapacity = -1
        });

    public UnitSystem Units => UnitSystem.Inches;

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

    public string Name => "Replay";

    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;

    #endregion

    // embedded helper class to read the binary format of some old logscanner implementations
    #region Private Methods

    private void ThreadMain(CancellationToken ct)
    {
        try
        {
            IsRunning = true;
            FillBuffer();
            var index = 0;
            while (index < PlaybackProfiles.Count)
            {
                ct.ThrowIfCancellationRequested();
                var profile = PlaybackProfiles[index];

                if (!AvailableProfiles.Post(profile))
                {
                    Logger.Error("Dropped profile.");
                }


                index++;
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
        }
    }
    

    private void FillBuffer()
    {
        string simFile;
        try
        {
            simFile = Config.File;
        }
        catch (Exception e)
        {
            Logger.Fatal(e, "Failed to get replay file name from configuration. ");
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
            Logger.Fatal(e, $"Failed to open/read replay file: {simFile}");
            throw;
        }
    }
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

    #endregion
}
