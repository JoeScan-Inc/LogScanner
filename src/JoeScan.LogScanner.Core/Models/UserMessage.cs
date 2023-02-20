using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Interfaces;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

public record UserMessage
{
    public UserMessage(object? sender, PluginMessageEventArgs e)
    {
        if (sender is IPlugin plugin)
        {
            Sender = plugin.Name;
        }
        else
        {
            Sender = "Internal";
        }

        Level = e.Level;
        Message = e.Message;
        DateTime = e.DateTime;
    }

    public string Sender { get; init; }
    public LogLevel Level { get; init; }

    public string Message { get; init; }
    public DateTime DateTime { get; init; }

    public UserMessage(string sender, LogLevel level, string message)
    {
        Sender = sender;
        Level = level;
        Message = message;
        DateTime = DateTime.Now;
    }
}
