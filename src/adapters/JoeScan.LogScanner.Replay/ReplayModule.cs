using Autofac;
using Autofac.Features.AttributeFilters;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Replay;
public class ReplayModule : Module 
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ReplayAdapter>().As<IScannerAdapter>().WithAttributeFiltering();
        
        builder.Register(c => new ConfigurationBuilder<IReplayAdapterConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetUserConfigLocation(), "adapters","replay","ReplayAdapter_user.json"))
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "adapters", "replay", "ReplayAdapter.json"))
            .Build()).As<IReplayAdapterConfig>().SingleInstance();
    }
}
