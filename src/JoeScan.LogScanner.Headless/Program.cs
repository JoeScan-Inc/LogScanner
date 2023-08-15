using JoeScan.LogScanner.Core.Models;
using System;
using System.Linq;
using System.Threading;


AutoResetEvent autoResetEvent = new AutoResetEvent(false);
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Cancelled");
    autoResetEvent.Set();
};


var engine = LogScannerEngine.Create();
engine.SetActiveAdapter(engine.AvailableAdapters.First(q=>q.Name.Contains("Replay"))); 
await engine.Start();
autoResetEvent.WaitOne();
engine.Stop();



