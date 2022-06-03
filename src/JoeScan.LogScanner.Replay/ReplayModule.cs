using Autofac;
using Autofac.Features.AttributeFilters;
using Config.Net;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Replay;
public class ReplayModule : Module 
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ReplayAdapter>().As<IScannerAdapter>().WithAttributeFiltering();
        builder.Register(c => new ConfigurationBuilder<IReplayAdapterConfig>()
            .UseIniFile("ReplayAdapter.ini")
            .Build()).As<IReplayAdapterConfig>().SingleInstance();
    }
}
