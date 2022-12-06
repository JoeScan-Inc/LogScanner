using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Replay;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<ReplayModule>();
    }
}
