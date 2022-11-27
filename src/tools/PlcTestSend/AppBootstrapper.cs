using Autofac;
using Autofac.Core;
using Autofac.Extras.NLog;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Shared;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PlcTestSend;

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
       

        // // -- Adapter Modules are now registered from assembly, see bottom --
        // // builder.RegisterModule<ReplayModule>();
        // // builder.RegisterModule<Js25Module>();
        // // builder.RegisterModule<Js50Module>();
        // // builder.RegisterModule<SyntheticDataModule>();
        //
        // builder.RegisterType<EngineViewModel>().AsSelf().SingleInstance();
        //
        // // the actual log scanner engine is in CoreModule
        // builder.RegisterModule<CoreModule>();
        // // logging
        // builder.RegisterModule<NLogModule>();
        // // notifier
        // builder.RegisterModule<NotifierModule>();
        //
        // // UI controls. We use the AutofacBootstrapper which registers all ViewModels and Views automatically,
        // // these are just special to warrant re-registration because we want to force them to be singletons
        // builder.RegisterType<StatusBarViewModel>().AsSelf().SingleInstance();
        // builder.RegisterType<ToolbarViewModel>().AsSelf().SingleInstance();
        // builder.RegisterType<TopAndSideViewModel>().AsSelf().SingleInstance();
        // builder.Register(c => new ConfigurationBuilder<ILogScannerConfig>()
        //     .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "LogScanner.Desktop.Config.json"))
        //     .Build()).SingleInstance();
        // // add vendor specific assemblies from a folder 


       // Assembly executingAssembly = Assembly.GetExecutingAssembly();

        // TODO: move this to module so headless can use it too
        // string applicationDirectory = Path.GetDirectoryName(executingAssembly.Location);
        // foreach (var file in Directory.GetFiles(Path.Combine(applicationDirectory!, "vendor"), "*.dll"))
        // {
        //     try
        //     {
        //         var vendorAssembly = Assembly.LoadFile(file);
        //         builder.RegisterAssemblyModules(new Assembly[]{vendorAssembly});
        //     }
        //     catch
        //     {
        //
        //     }
        // }

       
    }

   
}
