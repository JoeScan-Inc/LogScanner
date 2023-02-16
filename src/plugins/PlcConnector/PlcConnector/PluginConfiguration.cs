using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace PlcConnector;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<PlcConnectorModule>();
    }
}
