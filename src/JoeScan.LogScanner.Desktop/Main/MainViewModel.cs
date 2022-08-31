using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.Live3D;
using JoeScan.LogScanner.Desktop.LiveProfiles;
using JoeScan.LogScanner.Desktop.LogHistory;
using JoeScan.LogScanner.Desktop.LogProperties;
using JoeScan.LogScanner.Desktop.StatusBar;
using JoeScan.LogScanner.Desktop.Toolbar;
using JoeScan.LogScanner.Desktop.TopAndSide;
using JoeScan.LogScanner.Shared.Log3D;
using NLog;
using System.Windows;

namespace JoeScan.LogScanner.Desktop.Main;

public class MainViewModel : Screen
{
    public IUserNotifier Notifier { get; }
    public StatusBarViewModel StatusBar { get; }
    public ToolbarViewModel Toolbar { get; }
    public TopAndSideViewModel TopAndSide { get; }
    public LiveProfileViewModel LiveView { get; }
    public LogHistoryViewModel LogHistory { get; }
    public Live3DViewModel Live3D { get; }
    public LogPropertiesViewModel LogProperties { get; }
    public ILogger Logger { get; }
    public ILogScannerConfig Config { get; }
    public Visibility IsBusy => Notifier.IsBusy ? Visibility.Visible : Visibility.Hidden;
    
    public MainViewModel(
        IUserNotifier notifier,
        ILogger logger,
        ILogScannerConfig config,
       
        StatusBarViewModel statusBar,
        ToolbarViewModel toolbar,
        TopAndSideViewModel topAndSide,
        LiveProfileViewModel liveView,
        LogHistoryViewModel logHistory,
        Live3DViewModel live3D,
        LogPropertiesViewModel logProperties
      )
    {
        Notifier = notifier;
        StatusBar = statusBar;
        Toolbar = toolbar;
        TopAndSide = topAndSide;
        LiveView = liveView;
        LogHistory = logHistory;
        Live3D = live3D;
        LogProperties = logProperties;
        Logger = logger;
        Config = config;

       
    }
  


   

   

}
