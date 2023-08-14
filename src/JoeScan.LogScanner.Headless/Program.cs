using Autofac;
using JoeScan.LogScanner.Core;
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

var builder = new ContainerBuilder();
builder.RegisterModule<CoreModule>();
var container = builder.Build();

using var scope = container.BeginLifetimeScope();
var engine= scope.Resolve<LogScannerEngine>();
engine.SetActiveAdapter(engine.AvailableAdapters.First()); // we only have the replay adapter registered
engine.Start();
autoResetEvent.WaitOne();
engine.Stop();



