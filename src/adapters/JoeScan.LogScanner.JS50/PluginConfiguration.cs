using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Js50;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterType<Js50Adapter>().As<IScannerAdapter>();
    }
}
