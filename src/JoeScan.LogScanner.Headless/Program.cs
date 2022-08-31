using Autofac;
using Autofac.Extras.NLog;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.SyntheticDataAdapter;
using NLog;
using NLog.Config;
using NLog.Targets;

AutoResetEvent autoResetEvent = new AutoResetEvent(false);
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Cancelled");
    autoResetEvent.Set();
};
EnableLogging();
var builder = new ContainerBuilder();
// builder.RegisterModule<ReplayModule>();
builder.RegisterModule<SyntheticDataModule>();
builder.RegisterModule<CoreModule>();
builder.RegisterModule<NLogModule>();

var container = builder.Build();

using var scope = container.BeginLifetimeScope();
var engine= scope.Resolve<LogScannerEngine>();
engine.SetActiveAdapter(engine.AvailableAdapters.First()); // we only have the replay adapter registered
engine.Start();

autoResetEvent.WaitOne();
engine.Stop();


void EnableLogging()
 {
     var config = new LoggingConfiguration();
     const string layoutString = "[${longdate:useUTC=false}][${level:uppercase=true}]: ${message} ${exception:innerFormat=tostring:maxInnerExceptionLevel=10:separator=,:format=tostring}";
    
     var consoleTarget = new ColoredConsoleTarget()
     {
         
         Layout = layoutString,
         
     };
     config.AddTarget("console", consoleTarget);
     config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));

     LogManager.Configuration = config;
     LogManager.ReconfigExistingLoggers();
 }
