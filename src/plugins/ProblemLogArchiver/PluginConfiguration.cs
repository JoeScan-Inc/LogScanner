using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace ProblemLogArchiver;

public class PluginConfiguration : IPluginFactory
{
    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<ProblemLogArchiverModule>();
    }
}
