using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Core.Adapters.JS50;

public class Js50Module : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Js50Adapter>().As<IScannerAdapter>();
        builder.Register(c => new ConfigurationBuilder<IJs50AdapterConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(),
                "Adapters", "JS50",
                "js50adapter.json"))
            .Build()).As<IJs50AdapterConfig>().SingleInstance();
        
    }
}
