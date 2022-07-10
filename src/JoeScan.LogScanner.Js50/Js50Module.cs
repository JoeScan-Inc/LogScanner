using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Js50;

public class Js50Module : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => new ConfigurationBuilder<IJs50AdapterConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetUserConfigLocation(), "adapters", "js50", "js50adapter_user.json"))
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(), "adapters", "js50", "js50adapter.json"))
            .Build()).As<IJs50AdapterConfig>().SingleInstance();
        builder.RegisterModule<CoreModule>();
        builder.RegisterType<Js50Adapter>().As<IScannerAdapter>();
        builder.RegisterType<ScanSyncReceiverThread>().AsSelf();

    }
}
