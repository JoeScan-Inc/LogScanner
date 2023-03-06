using Autofac.Extras.NLog;
using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Shared.Notifier;
using JoeScan.LogScanner.Shared;
using MvvmDialogs;
using RawViewer.Shell;
using System;
using System.IO;
using System.Windows;

namespace RawViewer;

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
        builder.RegisterType<DialogService>().As<IDialogService>();
        builder.RegisterModule<NLogModule>();
        builder.Register(c => new ConfigurationBuilder<IRawViewerConfig>()
            .UseJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RawViewerConfig.json"))
            .Build()).SingleInstance();
        //
        // builder.RegisterType<EngineViewModel>().AsSelf().SingleInstance();
        //
        // // the actual log scanner engine is in CoreModule
        // builder.RegisterModule<CoreModule>();
        // // logging

        // // notifier
        // builder.RegisterModule<NotifierModule>();
        //
        // // UI controls. We use the AutofacBootstrapper which registers all ViewModels and Views automatically,
        // // these are just special to warrant re-registration because we want to force them to be singletons
        // builder.RegisterType<StatusBarViewModel>().AsSelf().SingleInstance();
        // builder.RegisterType<ToolbarViewModel>().AsSelf().SingleInstance();
        // builder.RegisterType<TopAndSideViewModel>().AsSelf().SingleInstance();
        // builder.Register(c => new ConfigurationBuilder<ILogScannerConfig>()
        //     .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(),
        //         "LogScanner.Desktop.Config.json"))
        //     .Build()).SingleInstance();
    }
}

