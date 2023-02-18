using JoeScan.LogScanner.LogReview.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JoeScan.LogScanner.LogReview
{
    public partial class App
    {
        #region Constants and Fields

        private const string UNIQUE_EVENT_NAME = "{4C3565CD-0ACF-45C1-B017-8F55B5088F2D}";
        private const string UNIQUE_MUTEX_NAME = "{19F70401-6AA0-4D27-82E7-FF3866A67212}";
        private EventWaitHandle? eventWaitHandle;
        private Mutex? mutex;

        #endregion

        #region Methods

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

        #endregion
    }
}
