using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.SyntheticDataAdapter;
public class SyntheticDataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SyntheticDataAdapter>().As<IScannerAdapter>();
        builder.RegisterType<FakeLogGenerator>().AsSelf();
        builder.Register(c => new ConfigurationBuilder<ISyntheticDataAdapterConfig>()
            .UseJsonFile(@"SyntheticDataAdapter.json")
            .Build()).As<ISyntheticDataAdapterConfig>().SingleInstance();
    }
}
