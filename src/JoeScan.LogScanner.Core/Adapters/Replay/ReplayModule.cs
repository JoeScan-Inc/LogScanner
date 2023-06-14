using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Core.Adapters.Replay;
public class ReplayModule : Module 
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ReplayAdapter>().As<IScannerAdapter>();
        
        builder.Register(c => new ConfigurationBuilder<IReplayAdapterConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "ReplayAdapter.json"))
            .Build()).As<IReplayAdapterConfig>().SingleInstance();
    }
}
