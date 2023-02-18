using System.IO;
using Autofac;
using Autofac.Extras.NLog;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.LogReview.Shell;
using JoeScan.LogScanner.Shared;
using System.Windows;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.LogReview.CrossSection;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.SectionTable;
using JoeScan.LogScanner.LogReview.Settings;
using JoeScan.LogScanner.Shared.Log3D;
using MvvmDialogs;

namespace JoeScan.LogScanner.LogReview;

public class AppBootstrapper : AutofacBootstrapper
{
    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        DisplayRootViewFor<ShellViewModel>();
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.Register(c => new ConfigurationBuilder<ILogReviewSettings>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "LogReviewConfig.json"))
            .Build()).SingleInstance();

        // the actual log scanner engine is in CoreModule
        builder.RegisterModule<CoreModule>();
        // logging
        builder.RegisterModule<NLogModule>();
        // use the reviewer object as an observable that holds the loaded log data
        builder.RegisterType<LogReviewer>().AsSelf().As<ILogModelObservable>().SingleInstance();
        builder.RegisterType<DialogService>().As<IDialogService>();
    }
}

