using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace CylinderFitter;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<CylinderFitterModule>();
    }
}
