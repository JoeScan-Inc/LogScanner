using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using System.Reflection;
using Module = Autofac.Module;

namespace JoeScan.LogScanner.SyntheticDataAdapter;
public class SyntheticDataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SyntheticDataAdapter>().As<IScannerAdapter>();
        builder.RegisterType<FakeLogGenerator>().AsSelf();
        builder.Register(c => new ConfigurationBuilder<ISyntheticDataAdapterConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(),"SyntheticDataAdapter.json"))
            .Build()).As<ISyntheticDataAdapterConfig>().SingleInstance();
    }
}
