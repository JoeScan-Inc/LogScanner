using NLog;

namespace JoeScan.LogScanner.Core.Events;

public class AdapterMessageEventArgs : EventArgs
{
    public string Message { get; }
    public NLog.LogLevel Level { get; }

    public AdapterMessageEventArgs(LogLevel level, string message)
    {
        Message = message;
        Level = level;
    }
}
