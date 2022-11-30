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
        builder.Register(c =>
        {
            // since we're loaded into the main engine's application context,
            // we won't find the config file next to the plugin dll. 
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return new ConfigurationBuilder<ISyntheticDataAdapterConfig>()
                .UseJsonFile(Path.Combine(assemblyPath!,"SyntheticDataAdapter.json"))
                .Build();
        }).As<ISyntheticDataAdapterConfig>().SingleInstance();
    }
}
