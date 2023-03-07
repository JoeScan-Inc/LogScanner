using Autofac;
using JoeScan.LogScanner.Core.Interfaces;
namespace SamplePlugin;

public class PluginConfiguration : IPluginFactory
{
    // the plugin factory calls Configure when discovering the plugins.
    // Here you need to register the Module that contains all your registrations for the plugin

    // in most cases, this does not need to be modified. 

    public void Configure(ContainerBuilder builder)
    {
        builder.RegisterModule<SamplePluginModule>();
    }
}
