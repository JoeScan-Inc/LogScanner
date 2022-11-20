using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Js25;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<Js25Module>();
    }
}
