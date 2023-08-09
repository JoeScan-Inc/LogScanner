using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using Module = Autofac.Module;

namespace JoeScan.LogScanner.Core.Adapters.SyntheticData;
public class SyntheticDataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SyntheticDataAdapter>().As<IScannerAdapter>();
        builder.RegisterType<FakeLogGenerator>().AsSelf();
        builder.Register(c => new ConfigurationBuilder<ISyntheticDataAdapterConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(),
                "Adapters", "SyntheticData",
                "SyntheticDataAdapter.json"))
            .Build()).As<ISyntheticDataAdapterConfig>().SingleInstance();
    }
}
