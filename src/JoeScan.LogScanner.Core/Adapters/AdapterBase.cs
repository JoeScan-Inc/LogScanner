using JoeScan.LogScanner.Core.Events;
using NLog;

namespace JoeScan.LogScanner.Core.Adapters;

public class AdapterBase
{
    private ILogger logger;
    protected AdapterBase(ILogger logger)
    {
        this.logger = logger;
    }

    public event EventHandler<PluginMessageEventArgs>? PluginMessage ;
    protected virtual void OnAdapterMessage(PluginMessageEventArgs e)
    {
        PluginMessage?.Invoke(this, e);
    }
    protected virtual void DiagnosticMessage(string message, LogLevel level)
    {
        logger.Log(level, message);
        OnAdapterMessage(new PluginMessageEventArgs(level, message));
    }
}
