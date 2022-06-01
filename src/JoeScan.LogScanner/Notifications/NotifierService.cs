using JoeScan.LogScanner.Core.Interfaces;
using System;
using ToastNotifications;
using ToastNotifications.Messages;

namespace JoeScan.LogScanner.Notifications;

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
                OnBusyChanged();
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
        toastNotifier.ShowInformation(message);
    }

    public void Warn(string message)
    {
        toastNotifier.ShowWarning(message);
    }

    public void Error(string message)
    {
        toastNotifier.ShowError(message);
    }

    public void Success(string message)
    {
        toastNotifier.ShowSuccess(message);
    }

   

    protected virtual void OnBusyChanged()
    {
        BusyChanged?.Invoke(this, EventArgs.Empty);
    }
}
