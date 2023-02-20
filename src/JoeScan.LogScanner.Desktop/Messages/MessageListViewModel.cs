using Caliburn.Micro;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Desktop.Config;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace JoeScan.LogScanner.Desktop.Messages;

public class MessageListViewModel : Screen
{
    private ICollectionView collectionView;
    private LogLevel selectedLevel;
    private int maxNumMessages = 300;
    public NLog.LogLevel SelectedLevel
    {
        get => selectedLevel;
        set
        {
            if (Equals(value, selectedLevel))
            {
                return;
            }

            selectedLevel = value;
            NotifyOfPropertyChange(() => SelectedLevel);
            collectionView.Refresh();

        }
    }

    public KeyValuePair<NLog.LogLevel, string>[] SelectableLevels { get; } = new KeyValuePair<LogLevel, string>[]
    {
        new KeyValuePair<LogLevel, string>(LogLevel.Trace, "Trace"),
        new KeyValuePair<LogLevel, string>(LogLevel.Debug, "Debug"),
        new KeyValuePair<LogLevel, string>(LogLevel.Info, "Info"),
        new KeyValuePair<LogLevel, string>(LogLevel.Trace, "Trace"),
        new KeyValuePair<LogLevel, string>(LogLevel.Warn, "Warn"),
        new KeyValuePair<LogLevel, string>(LogLevel.Error, "Error"),
        new KeyValuePair<LogLevel, string>(LogLevel.Fatal, "Fatal")
    };

    public ObservableCollection<MessageItem> MessageItems { get; set; } = new BindableCollection<MessageItem>();

    public MessageListViewModel(LogScannerEngine engine,
        ILogScannerConfig config)
    {
        engine.PluginMessageReceived += OnMessageReceived;
        collectionView = CollectionViewSource.GetDefaultView(MessageItems);
        collectionView.Filter += o =>
        {
            if (o is MessageItem item)
            {
                if (item.Level >= SelectedLevel)
                {
                    return true;
                }
            }

            return false;
        };
        SelectedLevel = LogLevel.Info;
        maxNumMessages = config.MessageListConfig.MaxLength;
    }

    private void OnMessageReceived(object? sender, PluginMessageEventArgs e)
    {
        if (sender is IPlugin plugin)
        {
            MessageItems.Insert(0, new MessageItem(plugin.Name, e.Level, e.Message));
        }
        else
        {
            MessageItems.Insert(0, new MessageItem("Internal", e.Level, e.Message));
        }

        if (MessageItems.Count > maxNumMessages)
        {
            MessageItems.RemoveAt(maxNumMessages);
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
