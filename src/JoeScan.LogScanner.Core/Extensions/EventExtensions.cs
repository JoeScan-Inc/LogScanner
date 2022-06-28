using System.ComponentModel;

namespace JoeScan.LogScanner.Core.Extensions;

public  static class EventExtensions
{
    /// <summary>Raises the event (on the UI thread if available).</summary>
    /// <param name="multicastDelegate">The event to raise.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An EventArgs that contains the event data.</param>
    /// <returns>The return value of the event invocation or null if none.</returns>
    public static object Raise<TEventArgs>(this MulticastDelegate? multicastDelegate, object sender, TEventArgs e)
        where TEventArgs : EventArgs
    {
        object? retVal = null;

        var threadSafeMulticastDelegate = multicastDelegate;
        if (threadSafeMulticastDelegate != null)
        {
            foreach (var d in threadSafeMulticastDelegate.GetInvocationList())
            {
                if ((d.Target is ISynchronizeInvoke { InvokeRequired: true } synchronizeInvoke))
                {
                    retVal = synchronizeInvoke.EndInvoke(synchronizeInvoke.BeginInvoke(d, new[] { sender, e }))!;
                }
                else
                {
                    retVal = d.DynamicInvoke(sender, e)!;
                }
            }
        }

        return retVal!;
    }
}
