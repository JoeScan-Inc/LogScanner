using Caliburn.Micro;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace JoeScan.LogScanner.Desktop.Messages;

public class MessageListViewModel : Screen
{
    public ObservableCollection<MessageItem> MessageItems { get; set; } = new BindableCollection<MessageItem>();
    public MessageListViewModel(LogScannerEngine engine)
    {
        engine.AdapterMessageReceived += OnMessageReceived;
        // Messages.Add(new MessageItem("JS-25 Adapter",LogLevel.Error, "Some Message"));
    }

    private void OnMessageReceived(object? sender, AdapterMessageEventArgs e)
    {
        if (sender is IScannerAdapter adapter)
        {
            MessageItems.Insert(0,new MessageItem(adapter.Name, e.Level, e.Message));
        }
        else
        {
            MessageItems.Insert(0, new MessageItem("Internal", e.Level, e.Message));
        }
    }
}

public class MessageItem
{
    public string Sender { get; init; }
    public NLog.LogLevel Level { get; init; }

    public string Message { get; init; }
    public DateTime DateTime { get; init; }

    public MessageItem(string sender, LogLevel level, string message)
    {
        Sender = sender;
        Level = level;
        Message = message;
        DateTime = DateTime.Now;
    }
}
