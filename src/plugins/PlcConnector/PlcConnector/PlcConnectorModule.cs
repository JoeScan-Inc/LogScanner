using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Interfaces;
using System.Reflection;
using Module = Autofac.Module;

namespace PlcConnector;

public class PlcConnectorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlcConnectorPlugin>().AsImplementedInterfaces().SingleInstance();
        builder.Register(c =>
        {
            // since we're loaded into the main engine's application context,
            // we won't find the config file next to the plugin dll. 
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return new ConfigurationBuilder<IPlcConnectorConfig>()
                .UseIniFile(Path.Combine(assemblyPath!, "PlcConnector.ini"))
                .Build();
        }).As<IPlcConnectorConfig>().SingleInstance();

    }
}
