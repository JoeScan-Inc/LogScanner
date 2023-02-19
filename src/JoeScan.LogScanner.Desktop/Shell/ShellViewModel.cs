using AdonisUI.Controls;
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.LiveProfiles;
using JoeScan.LogScanner.Desktop.LogHistory;
using JoeScan.LogScanner.Desktop.Messages;
using JoeScan.LogScanner.Desktop.StatusBar;
using JoeScan.LogScanner.Desktop.Toolbar;
using JoeScan.LogScanner.Desktop.TopAndSide;
using JoeScan.LogScanner.Shared.Live3D;
using JoeScan.LogScanner.Shared.LogProperties;
using JoeScan.LogScanner.Shared.Notifier;
using NLog;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace JoeScan.LogScanner.Desktop.Shell;

public class ShellViewModel : Screen
{
    public IUserNotifier Notifier { get; }
    public StatusBarViewModel StatusBar { get; }
    public ToolbarViewModel Toolbar { get; }
    public TopAndSideViewModel TopAndSide { get; }
    public LiveProfileViewModel LiveView { get; }
    public LogHistoryViewModel LogHistory { get; }
    public Live3DViewModel Live3D { get; }
    public LogPropertiesViewModel LogProperties { get; }
    public MessageListViewModel Messages { get; }
    public LogScannerEngine Engine { get; }
    public ILogger Logger { get; }
    public ILogScannerConfig Config { get; }
    public Visibility IsBusy => Notifier.IsBusy ? Visibility.Visible : Visibility.Hidden;

    public ShellViewModel(
        IUserNotifier notifier,
        ILogger logger,
        ILogScannerConfig config,
        StatusBarViewModel statusBar,
        ToolbarViewModel toolbar,
        TopAndSideViewModel topAndSide,
        LiveProfileViewModel liveView,
        LogHistoryViewModel logHistory,
        Live3DViewModel live3D,
        LogPropertiesViewModel logProperties,
        MessageListViewModel messages,
        LogScannerEngine engine
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
        Messages = messages;
        LogProperties.SetDisplayUnits(config.Units);
        Engine = engine;
        Logger = logger;
        Config = config;
        Engine.LogModelBroadcastBlock.LinkTo(new ActionBlock<LogModelResult>((result) =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!result.IsValidModel)
                {
                    Notifier.Error($"Log Model could not be generated for RawLog #{result.LogNumber}.");
                }
            });
        }));
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await CheckForEngineRunning(cancellationToken);
    }

    public Task<bool> CheckForEngineRunning(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.Run((() =>
        {
            if (Engine.IsRunning)
            {
                var messageBox = new MessageBoxModel
                {
                    Text = "LogScanner is still running. Do you really want to stop scanning and exit?",
                    Caption = "Confirm Exit",
                    Icon = MessageBoxImage.Question,
                    Buttons = new[]
                    {
                        MessageBoxButtons.Cancel("Exit"),
                        MessageBoxButtons.Yes("Cancel"),
                    }
                };
                return MessageBoxResult.Yes != Application.Current.Dispatcher.Invoke(() => MessageBox.Show(messageBox));
            }
            return true;
        }));
    }

    public void ExitApplication()
    {
        TryCloseAsync();
    }





}
