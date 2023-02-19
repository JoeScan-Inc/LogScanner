using JoeScan.LogScanner.Core.Extensions;
using System.Windows;
using ToastNotifications.Messages;

namespace JoeScan.LogScanner.Shared.Notifier;

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
    private readonly ToastNotifications.Notifier toastNotifier;
    private bool isBusy;


    public NotifierService(ToastNotifications.Notifier toastNotifier)
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
