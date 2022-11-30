using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoeScan.LogScanner.Core.Helpers
{
    public static class RecurringTaskHelper
    {
        public static void RecurringTask(Action? action, TimeSpan interval, CancellationToken token)
        {
            if (action == null)
                return;
            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(interval, token);
                }
            }, token);
        }
    }
}
