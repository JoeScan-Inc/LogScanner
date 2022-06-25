using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.IO.Compression;

namespace JoeScan.LogScanner.Core.Models;

public class RawLogArchiver : ILogArchiver
{
    public ILogger Logger { get; }
    private IRawLogArchiverConfig config;
    public RawLogArchiver(ICoreConfig coreConfig, ILogger logger)
    {
        Logger = logger;
        config = coreConfig.RawLogArchiverConfig;
        Logger.Debug("Created RawLogArchiver");
    }

    public void ArchiveLog(RawLog rawLog)
    {
        if (string.IsNullOrEmpty(config.Location))
        {
            Logger.Debug("No location set for archiving raw logs.");
            return;
        }

        if (!Directory.Exists(config.Location))
        {
            try
            {
                Directory.CreateDirectory(config.Location);
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to create raw log archive directory (\"{config.Location}\"). Error was: {e.Message}");
                return;
            }
        }

        var fileName = Path.Combine(config.Location, $"RawLog_{rawLog.LogNumber:0000}.loga");
        using var fs = new FileStream(fileName, FileMode.Create);
        using var gzip = new GZipStream(fs, CompressionMode.Compress);
        using var writer = new BinaryWriter(gzip);
        try
        {
            rawLog.Write(writer);

        }
        catch (Exception e)
        {
            Logger.Warn($"Failed to write raw log {rawLog.LogNumber} to archive. Error was: {e.Message}");
        }
    }

    public RawLog UnarchiveLog(Guid logId)
    {
        return null;
    }
}
