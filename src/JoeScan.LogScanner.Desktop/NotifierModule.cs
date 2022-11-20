using Autofac;
using JoeScan.LogScanner.Desktop.Notifications;
using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace JoeScan.LogScanner.Desktop;

public class NotifierModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
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
