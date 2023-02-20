using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Helpers;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using Nini.Config;
using NLog;
using System.IO.Compression;

namespace ProblemLogArchiver;
public class ProblemLogArchiver : ILogModelConsumerPlugin, IDisposable
{
    #region Injected Properties

    public IProblemLogArchiverConfig Config { get; }
    public ILogger Logger { get; }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {

    }

    #endregion

    #region ILogModelConsumerPlugin

    public bool IsInitialized { get; private set; } = false;

    public void Initialize()
    {
        SendInfo("Initializing ProblemLogArchiver");
        if (!Config.Enabled)
        {
            SendWarning("ProblemLogArchiver disabled in config file.");
            return;
        }
        if (!String.IsNullOrEmpty(Config.ArchiveLocation))
        {

            if (Directory.CreateDirectory(Config.ArchiveLocation).Exists)
            {
                IsInitialized = true;
                SendInfo($"ProblemLogArchiver successfully initialized with Location {Config.ArchiveLocation}");
                return;
            }

        }
        IsInitialized = false;
        SendWarning("ProblemLogArchiver disabled: ArchiveLocation not found.");
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
            SendWarning($"Failed to write raw log {logModel.RawLog.LogNumber} to archive. Error was: {e.Message}");
        }
    }

    #endregion

    #region IPlugin Implementation

    public string Name { get; } = "ProblemLogArchiver";
    public uint VersionMajor => 1;
    public uint VersionMinor => 0;
    public uint VersionPatch => 0;
    public Guid Id { get; } = Guid.Parse("{7700AE11-1A28-465B-B31B-D8F77117E715}");

    public event EventHandler<PluginMessageEventArgs>? PluginMessage;

    #endregion

    #region Lifecycle

    public ProblemLogArchiver(IProblemLogArchiverConfig config, ILogger logger)
    {
        Config = config;
        Logger = logger;
    }

    #endregion

    #region Event Invocation

    protected  virtual void OnPluginMessage(PluginMessageEventArgs e)
    {
        PluginMessage?.Invoke(this, e);
    }
    void OnPluginMessage(LogLevel level, string msg)
    {
        PluginMessage?.Invoke(this, new PluginMessageEventArgs(level,msg));
    }
   
    #endregion
    private void SendInfo(string message)
    {
        OnPluginMessage(LogLevel.Info, message);
        Logger.Info(message);
    }

    private void SendDebug(string message)
    {
        OnPluginMessage(LogLevel.Debug, message);
        Logger.Debug(message);
    }


    private void SendWarning(string message)
    {
        OnPluginMessage(LogLevel.Warn, message);
        Logger.Warn(message);
    }

    private void SendError(string message)
    {
        OnPluginMessage(LogLevel.Error, message);
        Logger.Error(message);
    }
}
