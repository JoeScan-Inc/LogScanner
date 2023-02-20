using NLog;

namespace JoeScan.LogScanner.Core.Events;

public class PluginMessageEventArgs : EventArgs
{
    public string Message { get; }
    public NLog.LogLevel Level { get; }

    public PluginMessageEventArgs(LogLevel level, string message)
    {
        Message = message;
        Level = level;
    }
}
