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
    private object locker = new object();

    public LogLevel SelectedLevel
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

    public ObservableCollection<UserMessage> MessageItems { get; init; }

    public MessageListViewModel(LogScannerEngine engine,
        ILogScannerConfig config)
    {
        BindingOperations.EnableCollectionSynchronization(engine.PluginMessages, locker);
        MessageItems = engine.PluginMessages;
        collectionView = CollectionViewSource.GetDefaultView(MessageItems);
        collectionView.Filter += o =>
        {
            if (o is UserMessage item)
            {
                if (item.Level >= SelectedLevel)
                {
                    return true;
                }
            }

            return false;
        };
        SelectedLevel = LogLevel.Info;
    }
}
