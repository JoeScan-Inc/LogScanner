using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Interfaces;
using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Messages;

namespace JoeScan.LogScanner.Desktop.Notifications;

public class NotifierService : IUserNotifier
{
    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (isBusy != value)
            {
                isBusy = value;
                BusyChanged.Raise(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? BusyChanged;
    private readonly Notifier toastNotifier;
    private bool isBusy;


    public NotifierService(Notifier toastNotifier)
    {
        this.toastNotifier = toastNotifier;
    }

    public void Info(string message)
    {
        Application.Current.Dispatcher.Invoke(new Action(() => { toastNotifier.ShowInformation(message); }));
        
    }

    public void Warn(string message)
    {
        Application.Current.Dispatcher.Invoke(new Action(() => { toastNotifier.ShowWarning(message); }));
    }

    public void Error(string message)
    {
        Application.Current.Dispatcher.Invoke(new Action(() => { toastNotifier.ShowError(message); }));
    }

    public void Success(string message)
    {
        Application.Current.Dispatcher.Invoke(new Action(() => { toastNotifier.ShowSuccess(message); }));
    }
   
}
