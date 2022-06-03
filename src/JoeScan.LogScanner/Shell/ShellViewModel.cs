using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LiveProfiles;
using JoeScan.LogScanner.Log3D;
using JoeScan.LogScanner.LogHistory;
using JoeScan.LogScanner.LogProperties;
using JoeScan.LogScanner.Notifications;
using JoeScan.LogScanner.StatusBar;
using JoeScan.LogScanner.Toolbar;
using JoeScan.LogScanner.TopAndSide;
using System.Windows;
using JoeScan.LogScanner.Config;

namespace JoeScan.LogScanner.Shell;

public class ShellViewModel : Screen
{
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

    public ShellViewModel(
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
    }

    public Visibility IsBusy => Notifier.IsBusy ? Visibility.Visible : Visibility.Hidden;
}
