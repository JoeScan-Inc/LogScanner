using Autofac;
using Autofac.Extras.NLog;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Replay;
using JoeScan.LogScanner.Js50;
using JoeScan.LogScanner.Notifications;
using JoeScan.LogScanner.Shared;
using JoeScan.LogScanner.Shell;
using JoeScan.LogScanner.StatusBar;
using JoeScan.LogScanner.Toolbar;
using JoeScan.LogScanner.TopAndSide;
using Nini.Config;
using System;
using System.Windows;
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
        // the generic, unkeyed IConfigSource needs to be registered first, as the modules 
        // register the same, but keyed
        builder.Register(c => new IniConfigSource("LogScanner.ini")).Keyed<IConfigSource>("LogScanner.ini").SingleInstance();
        // -- Adapter Modules --
        // only one adapter should be registered. The adapter module 
        // must provide at least one registration for an IScannerAdapter

       //  builder.RegisterModule<ReplayModule>();
        // builder.RegisterModule<Js25Module>();
        builder.RegisterModule<Js50Module>();

        builder.RegisterModule<CoreModule>();
        builder.RegisterModule<NLogModule>();
        builder.RegisterType<StatusBarViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ToolbarViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<TopAndSideViewModel>().AsSelf().SingleInstance();
        // builder.RegisterType<LiveProfileViewModel>().As<ILogAssembler>().SingleInstance();
        builder.RegisterType<FlightsAndWindowFilter>().As<IFlightsAndWindowFilter>();
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
