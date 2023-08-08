using Autofac;
using Autofac.Extras.NLog;
using Config.Net;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.Engine;
using JoeScan.LogScanner.Desktop.Shell;
using JoeScan.LogScanner.Desktop.StatusBar;
using JoeScan.LogScanner.Desktop.Toolbar;
using JoeScan.LogScanner.Desktop.TopAndSide;
using JoeScan.LogScanner.Shared;
using JoeScan.LogScanner.Shared.Notifier;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

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
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "LogScanner.Desktop.Config.json"))
            .Build()).SingleInstance();

        builder.RegisterType<EngineViewModel>().AsSelf().SingleInstance();

        // the actual log scanner engine is in CoreModule
        builder.RegisterModule<CoreModule>();
        // logging
        builder.RegisterModule<NLogModule>();
        // notifier
        builder.RegisterModule<NotifierModule>();

        // UI controls. We use the AutofacBootstrapper which registers all ViewModels and Views automatically,
        // these are just special to warrant re-registration because we want to force them to be singletons
        builder.RegisterType<StatusBarViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ToolbarViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<TopAndSideViewModel>().AsSelf().SingleInstance();
        builder.Register(c => new ConfigurationBuilder<ILogScannerConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(),
                "LogScanner.Desktop.Config.json"))
            .Build()).SingleInstance();
    }
}
