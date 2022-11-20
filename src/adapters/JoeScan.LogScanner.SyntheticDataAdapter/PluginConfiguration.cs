using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.SyntheticDataAdapter;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<SyntheticDataModule>();
    }
}
