using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Helpers;
using NLog;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Models;

public class RawProfileDumper
{
    public RawDumperConfig Config { get; }
    private readonly ILogger logger;
    public TransformBlock<Profile, Profile> DumpBlock { get; }
    public string BaseName { get; set; } = String.Empty;

    private BlockingCollection<Profile>? dumpQueue = null;
    private FixedSizedQueue<Profile>? historyQueue = null;
    private bool isDumping;

    public bool IsEnabled { get; set; } = true;

    public RawProfileDumper(RawDumperConfig config, ILogger logger)
    {
        Config = config;
        this.logger = logger;
        isDumping = false;
        DumpBlock = new TransformBlock<Profile, Profile>(ProcessProfile,
            new ExecutionDataflowBlockOptions()
            {
                BoundedCapacity = -1, EnsureOrdered = true, MaxDegreeOfParallelism = 1
            });
        historyQueue = new FixedSizedQueue<Profile>(Config.HistorySize);
    }

    public void StartDumping()
    {
        if (dumpQueue != null || !IsEnabled)
        {
            return;
        }

        // check for file being open and such here
        if (Config.Location == String.Empty)
        {
            var msg = "Output directory must not be empty. Dumping disabled.";
            logger.Warn(msg);
            return;
        }

        if (!Directory.Exists(Config.Location))
        {
            try
            {
                Directory.CreateDirectory(Config.Location);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to create directory: {Config.Location}: {ex.Message}. Dumping disabled.");
                return;
            }
        }

        var fileName = CreateOutputFileName();
        // TODO: make this a background thread so it finishes before the application 
        // exits
        Task.Run(() =>
        {
            dumpQueue = new BlockingCollection<Profile>();


            using var fs = new FileStream(fileName, FileMode.Create);
            using var gzip = new GZipStream(fs, CompressionMode.Compress);
            using var writer = new BinaryWriter(gzip);
            var count = 0L;
            while (!dumpQueue.IsCompleted)
            {
                Profile? p = null;
                try
                {
                    p = dumpQueue.Take();
                }
                catch (InvalidOperationException)
                {
                    // we get here once CompleteAdding has been called on
                    // the queue. 
                    logger.Debug("Dumper queue closed for adding.");
                }

                if (p != null)
                {
                    p.Write(writer);
                    count++;
                }
            }

            logger.Debug("Dumper queue empty, exiting task.");
            gzip.Close();
            dumpQueue = null;
            if (count == 0)
            {
                // didn't record anything, delete file
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception)
                {
                    //unused
                }
            }
        });
    }

    private string CreateOutputFileName()
    {
        var t = DateTime.Now;
        // TODO: do a File.Exists to catch cases where the seconds are not enough to distinct
        return Path.Combine(Config.Location,
            $"{BaseName}{t.Year}_{t.Month}_{t.Day}_{t.Hour}_{t.Minute}_{t.Second}.raw");
    }

    public void StopDumping()
    {
        if (dumpQueue != null)
        {
            dumpQueue.CompleteAdding();
        }
    }

    private Profile ProcessProfile(Profile p)
    {
        if (dumpQueue != null && !dumpQueue.IsAddingCompleted && IsEnabled)
        {
            dumpQueue.Add((Profile)p.Clone());
        }

        if (Config is
            {
                HistorySize: > 0, HistoryEnabled: true, Location: not null
            } && !isDumping)
        {
            historyQueue?.Enqueue((Profile)p.Clone());
        }

        return p;
    }

    public void DumpHistory()
    {
        if (Config is
            {
                HistorySize: > 0, HistoryEnabled: true, Location: not null
            })
        {
            
            if (!Directory.Exists(Config.Location))
            {
                try
                {
                    Directory.CreateDirectory(Config.Location);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to create directory: {Config.Location}: {ex.Message}. Dumping disabled.");
                    return;
                }
            }
            using var fs = new FileStream(CreateOutputFileName(), FileMode.Create);
            using var gzip = new GZipStream(fs, CompressionMode.Compress);
            using var writer = new BinaryWriter(gzip);
            isDumping = true; // queue temporarily disabled
            while (historyQueue!.TryDequeue(out Profile? p))
            {
                p.Write(writer);
            }
        }

        isDumping = false;
    }
}
