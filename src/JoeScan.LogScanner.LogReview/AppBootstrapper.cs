using Autofac;
using Autofac.Extras.NLog;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.LogReview.Shell;
using JoeScan.LogScanner.Shared;
using System.Windows;

namespace JoeScan.LogScanner.LogReview;

public class AppBootstrapper : AutofacBootstrapper
{
    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        DisplayRootViewFor<ShellViewModel>();
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {

      
        // the actual log scanner engine is in CoreModule
        builder.RegisterModule<CoreModule>();
        // logging
        builder.RegisterModule<NLogModule>();
        // UI controls. We use the AutofacBootstrapper which registers all ViewModels and Views automatically,
        // these are just special to warrant re-registration because we want to force them to be singletons
        // builder.RegisterType<StatusBarViewModel>().AsSelf().SingleInstance();
        // builder.RegisterType<ToolbarViewModel>().AsSelf().SingleInstance();
        // builder.RegisterType<TopAndSideViewModel>().AsSelf().SingleInstance();
        //
        // // wrap the Notifier in a service 
        // builder.Register(c => new NotifierService(new Notifier(cfg => {
        //     cfg.PositionProvider = new WindowPositionProvider(
        //         parentWindow: Application.Current.MainWindow,
        //         corner: Corner.BottomLeft,
        //         offsetX: 10,
        //         offsetY: 10);
        //
        //     cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
        //         notificationLifetime: TimeSpan.FromSeconds(5),
        //         maximumNotificationCount: MaximumNotificationCount.FromCount(5));
        //
        //     cfg.Dispatcher = Application.Current.Dispatcher;
        //
        //     cfg.DisplayOptions.TopMost = true;
        //     cfg.DisplayOptions.Width = 250;
        //     // this will override the registration of MuteNotifier in the engine
        // }))).As<IUserNotifier>().SingleInstance();

    }
}

