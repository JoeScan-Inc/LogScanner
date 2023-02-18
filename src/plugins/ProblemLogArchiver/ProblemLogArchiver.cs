using JoeScan.LogScanner.Core.Helpers;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using Nini.Config;
using NLog;
using System.IO.Compression;

namespace ProblemLogArchiver;
public class ProblemLogArchiver : ILogModelConsumerPlugin, IDisposable
{
    public IProblemLogArchiverConfig Config { get; }
    public ILogger Logger { get; }

    #region IDisposable Implementation

    public void Dispose()
    {

    }

    #endregion

    #region ILogModelConsumerPlugin

    public string PluginName { get; } = "ProblemLogArchiver";
    public int VersionMajor => 1;
    public int VersionMinor => 0;
    public int VersionPatch => 0;
    public Guid Id { get; } = Guid.Parse("{7700AE11-1A28-465B-B31B-D8F77117E715}");

    public bool IsInitialized { get; private set; } = false;

    public void Initialize()
    {
        Logger.Debug("Initializing ProblemLogArchiver");
        if (!Config.Enabled)
        {
            Logger.Debug("ProblemLogArchiver disabled in config file.");
            return;
        }
        if (!String.IsNullOrEmpty(Config.ArchiveLocation))
        {

            if (Directory.CreateDirectory(Config.ArchiveLocation).Exists)
            {
                IsInitialized = true;
                Logger.Debug($"ProblemLogArchiver successfully initialized with Location {Config.ArchiveLocation}");
                return;
            }

        }
        IsInitialized = false;
        Logger.Debug("ProblemLogArchiver disabled: ArchiveLocation not found.");
    }

    public void Cleanup()
    {

    }

    public void Consume(LogModelResult logModel)
    {
        if (!IsInitialized || !Config.Enabled || logModel.IsValidModel)
        {
            return;
        }

        var fileName = Path.Combine(Config.ArchiveLocation, $"RawLog_{logModel.RawLog.LogNumber:0000}.{RawLogReaderWriter.DefaultExtension}");
        using var fs = new FileStream(fileName, FileMode.Create);
        using var gzip = new GZipStream(fs, CompressionMode.Compress);
        using var writer = new BinaryWriter(gzip);
        try
        {
            logModel.RawLog.Write(writer);
            logModel.RawLog.ArchiveFileName = fileName;
            if (Config.MaxCount > 0)
            {
                ArchiveCleaner.CleanupDirectory(Config.ArchiveLocation, Config.MaxCount);
            }
        }
        catch (Exception e)
        {
            Logger.Warn($"Failed to write raw log {logModel.RawLog.LogNumber} to archive. Error was: {e.Message}");
        }
    }

    #endregion

    public ProblemLogArchiver(IProblemLogArchiverConfig config, ILogger logger)
    {
        Config = config;
        Logger = logger;
    }
}
