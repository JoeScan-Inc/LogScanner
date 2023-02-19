using System.IO;
using Autofac;
using Autofac.Extras.NLog;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.LogReview.Shell;
using JoeScan.LogScanner.Shared;
using System.Windows;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.LogReview.Config;
using JoeScan.LogScanner.LogReview.CrossSection;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.SectionTable;
using JoeScan.LogScanner.Shared.Live3D;
using JoeScan.LogScanner.Shared.LogProperties;
using MvvmDialogs;
using JoeScan.LogScanner.Shared.Notifier;

namespace JoeScan.LogScanner.LogReview;

public class AppBootstrapper : AutofacBootstrapper
{
    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        DisplayRootViewFor<ShellViewModel>();
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.Register(c => new ConfigurationBuilder<ILogReviewConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "LogScanner.LogReview.Config.json"))
            .Build()).SingleInstance();

        // the actual log scanner engine is in CoreModule
        builder.RegisterModule<CoreModule>();
        // logging
        builder.RegisterModule<NLogModule>();
        // notifier
        builder.RegisterModule<NotifierModule>();
        // use the reviewer object as an observable that holds the loaded log data
        builder.RegisterType<LogReviewer>().AsSelf().As<ILogModelObservable>().SingleInstance();
        builder.RegisterType<DialogService>().As<IDialogService>();
        // re-register the Live View so it uses the constructor that does not require a whole log 
        // engine, so we don't pull all the other crap in that is not needed here
        builder.RegisterType<Live3DViewModel>().UsingConstructor();
        // re-register the LogProperties view so it uses the constructor that does not require a whole log 
        // engine, so we don't pull all the other crap in that is not needed here
        builder.RegisterType<LogPropertiesViewModel>().UsingConstructor();
    }
}

