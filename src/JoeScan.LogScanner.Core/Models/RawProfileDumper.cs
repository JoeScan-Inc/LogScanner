using NLog;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Models;

public class RawProfileDumper
{
    private readonly ILogger logger;
    public TransformBlock<Profile,Profile> DumpBlock { get;  }
    public string OutputDir { get; set; } = String.Empty;
    public string BaseName { get; set; } = String.Empty;
    private bool acceptNewProfiles = false;

    private BlockingCollection<Profile>? dumpQueue  = null;

    public RawProfileDumper(ILogger logger)
    {
        this.logger = logger;
        DumpBlock = new TransformBlock<Profile, Profile>(ProcessProfile);
    }

    public void StartDumping()
    {
        if (acceptNewProfiles)
        {
            return;
        }
        // check for file being open and such here
        if (OutputDir == String.Empty)
        {
            var msg = "Output directory must not be empty.";
            throw new ApplicationException();
        }

        if (!Directory.Exists(OutputDir))
        {
            try
            {
                Directory.CreateDirectory(OutputDir);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to create directory: {OutputDir}: {ex.Message}");
                throw;
            }
        }

        var fileName = CreateOutputFileName();
        Task.Run(() =>
        {
            dumpQueue = new BlockingCollection<Profile>();
            using var fs = new FileStream(fileName, FileMode.Create);
            using var writer = new BinaryWriter(fs);
            acceptNewProfiles = true;
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
                }
            }

            logger.Debug("Dumper queue empty, exiting task.");
            acceptNewProfiles = false;
            dumpQueue = null;
        });
    }

    private string CreateOutputFileName()
    {
        var t = DateTime.Now;
        return Path.Combine(OutputDir, $"{BaseName}{t.Year}_{t.Month}_{t.Day}_{t.Hour}_{t.Minute}_{t.Second}.raw");
    }

    public void StopDumping()
    {
        if (acceptNewProfiles)
        {
            acceptNewProfiles = false;
            dumpQueue!.CompleteAdding();
        }
    }
    private Profile ProcessProfile(Profile p)
    {
        if (acceptNewProfiles)
        {
            dumpQueue!.Add(p);
        }
        return p;
    }
}
