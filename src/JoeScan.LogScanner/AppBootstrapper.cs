using Autofac;
using Autofac.Extras.NLog;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Notifications;
using JoeScan.LogScanner.Shared;
using JoeScan.LogScanner.Shell;
using JoeScan.LogScanner.StatusBar;
using JoeScan.LogScanner.Toolbar;
using JoeScan.LogScanner.TopAndSide;
using System;
using System.Windows;
using Config.Net;
using JoeScan.LogScanner.Config;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Js25;
using JoeScan.LogScanner.Js50;
using JoeScan.LogScanner.Replay;
using JoeScan.LogScanner.SyntheticDataAdapter;
using System.IO;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace JoeScan.LogScanner;

public class AppBootstrapper : AutofacBootstrapper
{
    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        DisplayRootViewFor<ShellViewModel>();
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {

        builder.Register(c => new ConfigurationBuilder<ILogScannerConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetConfigLocation(),"LogScannerConfig.json"))
            .Build()).SingleInstance();

        // -- Adapter Modules --
        builder.RegisterModule<ReplayModule>();
        builder.RegisterModule<Js25Module>();
        builder.RegisterModule<Js50Module>();
        builder.RegisterModule<SyntheticDataModule>();
        
        // the actual log scanner engine is in CoreModule
        builder.RegisterModule<CoreModule>();
        // logging
        builder.RegisterModule<NLogModule>();
        // UI controls. We use the AutofacBootstrapper which registers all ViewModels and Views automatically,
        // these are just special to warrant re-registration because we want to force them to be singletons
        builder.RegisterType<StatusBarViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ToolbarViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<TopAndSideViewModel>().AsSelf().SingleInstance();
       
        // wrap the Notifier in a service 
        builder.Register(c => new NotifierService(new Notifier(cfg => {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomLeft,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(5),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;

            cfg.DisplayOptions.TopMost = true;
            cfg.DisplayOptions.Width = 250;
            // this will override the registration of MuteNotifier in the engine
        }))).As<IUserNotifier>().SingleInstance();

    }
}
