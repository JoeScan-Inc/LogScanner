using Autofac;
using Autofac.Extras.NLog;
using Config.Net;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.Engine;
using JoeScan.LogScanner.Desktop.Main;
using JoeScan.LogScanner.Desktop.Notifications;
using JoeScan.LogScanner.Desktop.Shell;
using JoeScan.LogScanner.Desktop.StatusBar;
using JoeScan.LogScanner.Desktop.Toolbar;
using JoeScan.LogScanner.Desktop.TopAndSide;
using JoeScan.LogScanner.Js25;
using JoeScan.LogScanner.Js50;
using JoeScan.LogScanner.Replay;
using JoeScan.LogScanner.Shared;
using JoeScan.LogScanner.SyntheticDataAdapter;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace JoeScan.LogScanner.Desktop;

public class AppBootstrapper : AutofacBootstrapper
{
    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        DisplayRootViewFor<ShellViewModel>();
    }

    protected override void OnExit(object sender, EventArgs e)
    {
        Container.Dispose();
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.Register(c => new ConfigurationBuilder<ILogScannerConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(),"LogScannerConfig.json"))
            .Build()).SingleInstance();

        // -- Adapter Modules --
        builder.RegisterModule<ReplayModule>();
        builder.RegisterModule<Js25Module>();
        builder.RegisterModule<Js50Module>();
        builder.RegisterModule<SyntheticDataModule>();

        builder.RegisterType<EngineViewModel>().AsSelf().SingleInstance();

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


        builder.Register(c => new ConfigurationBuilder<ILogScannerConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "LogScanner.Desktop.Config.json"))
            .Build()).SingleInstance();
        // add vendor specific assemblies from a folder 
        Assembly executingAssembly = Assembly.GetExecutingAssembly();

        // TODO: move this to module so headless can use it too
        string applicationDirectory = Path.GetDirectoryName(executingAssembly.Location);
        foreach (var file in Directory.GetFiles(Path.Combine(applicationDirectory!, "vendor"), "*.dll"))
        {
            try
            {
                var vendorAssembly = Assembly.LoadFile(file);
                builder.RegisterAssemblyModules(new Assembly[]{vendorAssembly});
            }
            catch
            {

            }
        }
    }
}
