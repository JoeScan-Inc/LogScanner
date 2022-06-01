using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Core.Models;

public class MuteNotifier :IUserNotifier
{
    public void Info(string message)
    {
    }

    public void Warn(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Success(string message)
    {
    }

    public bool IsBusy { get; set; }
    public event EventHandler? BusyChanged;

    protected virtual void OnBusyChanged()
    {
        BusyChanged?.Invoke(this, EventArgs.Empty);
    }
}
