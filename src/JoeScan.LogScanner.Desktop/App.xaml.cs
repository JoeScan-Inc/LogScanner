using JoeScan.LogScanner.Desktop.Shell;
using System;
using System.Threading;
using System.Windows;

namespace JoeScan.LogScanner.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constants and Fields

        private const string UNIQUE_EVENT_NAME = "{AC6A4BE3-8809-4E45-851D-0B503F34711B}";
        private const string UNIQUE_MUTEX_NAME = "{A2AD5377-E841-4A51-A79A-6B45A0BD0ACF}";
        private EventWaitHandle? eventWaitHandle;
        private Mutex? mutex;

        #endregion

        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            mutex = new Mutex(true, UNIQUE_MUTEX_NAME, out bool isOwned);
            eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UNIQUE_EVENT_NAME);

            //  warning suppression
            GC.KeepAlive(mutex);

            if (isOwned)
            {
                // Spawn a thread which will be waiting for our event
                var thread = new Thread(
                    () =>
                    {
                        while (this.eventWaitHandle.WaitOne())
                        {
                            Current.Dispatcher.BeginInvoke(
                                (Action)(() =>
                                {
                                    ((ShellView)Current.MainWindow).Activate();
                                }));
                        }
                    })
                {
                    // It is important mark it as background otherwise it will prevent app from exiting.
                    IsBackground = true
                };

                thread.Start();
                return;
            }

            // Notify other instance so it could bring itself to foreground.
            this.eventWaitHandle.Set();

            // Terminate this instance.
            this.Shutdown();
        }

    }
}
