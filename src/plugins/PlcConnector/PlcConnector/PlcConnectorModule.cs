using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace PlcConnector;

public class PlcConnectorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PlcConnectorPlugin>().As<ILogModelConsumerPlugin>().As<IDisposable>();
    }
}
