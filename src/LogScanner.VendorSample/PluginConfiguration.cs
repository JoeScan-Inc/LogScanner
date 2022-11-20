using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace LogScanner.VendorSample;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<VendorModule>();
    }
}
