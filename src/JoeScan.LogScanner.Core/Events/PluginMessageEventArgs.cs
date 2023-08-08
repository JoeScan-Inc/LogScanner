using NLog;

namespace JoeScan.LogScanner.Core.Events;

public class PluginMessageEventArgs : EventArgs
{
    public string Message { get; }
    public LogLevel Level { get; }
    public DateTime DateTime { get; init; }



    public PluginMessageEventArgs(LogLevel level, string message)
    {
        Message = message;
        Level = level;
        DateTime = DateTime.Now;
    }
}
