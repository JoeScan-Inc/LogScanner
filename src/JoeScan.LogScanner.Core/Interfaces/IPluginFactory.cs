using Autofac;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IPluginFactory
{
    void Configure(ContainerBuilder builder);
}
