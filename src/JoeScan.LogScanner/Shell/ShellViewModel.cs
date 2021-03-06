using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LiveProfiles;
using JoeScan.LogScanner.LogHistory;
using JoeScan.LogScanner.LogProperties;
using JoeScan.LogScanner.StatusBar;
using JoeScan.LogScanner.Toolbar;
using JoeScan.LogScanner.TopAndSide;
using System.Windows;
using JoeScan.LogScanner.Config;
using JoeScan.LogScanner.Shared.Log3D;
using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace JoeScan.LogScanner.Shell;

public class ShellViewModel : Screen
{
    public ILogger Logger { get; }
    public ILogScannerConfig Config { get; }
    public StatusBarViewModel StatusBar { get; }
    public ToolbarViewModel Toolbar { get; }
    public TopAndSideViewModel TopAndSide { get; }
    public LiveProfileViewModel LiveView { get; }
    public LogHistoryViewModel LogHistory { get; }
    public Log3DViewModel Log3D { get; }
    public LogPropertiesViewModel LogProperties { get; }
    public LogScannerEngine Engine { get; }
    public IUserNotifier Notifier { get; }

    public string Title => "JoeScan LogScanner";

    public ShellViewModel(
        ILogger logger,
        ILogScannerConfig config,
        StatusBarViewModel statusBar,
        ToolbarViewModel toolbar,
        TopAndSideViewModel topAndSide,
        LiveProfileViewModel liveView,
        LogHistoryViewModel logHistory,
        Log3DViewModel log3D,
        LogPropertiesViewModel logProperties,
        LogScannerEngine engine,
        IUserNotifier notifier)
    {
        Logger = logger;
        Config = config;
        StatusBar = statusBar;
        Toolbar = toolbar;
        TopAndSide = topAndSide;
        LiveView = liveView;
        LogHistory = logHistory;
        Log3D = log3D;
        LogProperties = logProperties;
        Engine = engine;
        Notifier = notifier;
        Notifier.BusyChanged+= (_, _) =>
        {
            Refresh();
        };
        if (!string.IsNullOrEmpty(config.ActiveAdapter))
        {
            var n = Engine.AvailableAdapters.FirstOrDefault(q => q.Name == config.ActiveAdapter);
            {
                if (n != null)
                {
                    try
                    {
                        Engine.SetActiveAdapter(n);
                        Notifier.Success($"Active Adapter: {n}");
                    }
                    catch (Exception e)
                    {
                        var msg = $"Error setting active adapter: {e.Message}";
                        Logger.Error(msg);
                        Notifier.Error(msg);
                        Engine.SetActiveAdapter(Engine.AvailableAdapters.First());
                        Notifier.Warn($"Fallback adapter: {Engine.ActiveAdapter!.Name}");
                    }
                }
            }
        }

        engine.LogModelBroadcastBlock.LinkTo(new ActionBlock<LogModel>(model =>
        {
            // needs to run on UI thread
            Application.Current.Dispatcher.BeginInvoke(() => log3D.CurrentLogModel = model);
        }));
    }

    public Visibility IsBusy => Notifier.IsBusy ? Visibility.Visible : Visibility.Hidden;
}
