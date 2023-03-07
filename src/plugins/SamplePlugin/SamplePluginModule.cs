using Autofac;
using JoeScan.LogScanner.Core.Interfaces;
using System.Reflection;
using Module = Autofac.Module;

namespace SamplePlugin;

public class SamplePluginModule : Module 
{
    protected override void Load(ContainerBuilder builder)
    {
        // here you need to explicitly register the interfaces that your plugin implements
        builder.RegisterType<SamplePlugin>().As<ILogModelConsumerPlugin>().As<IDisposable>().SingleInstance();
        // or use AsImplementedInterfaces() - this is more convenient when your plugin 
        // implements other interfaces as well, such as IHeartBeatSubscriber or ILogStatusEventConsumer
        // builder.RegisterType<SamplePlugin>().AsImplementedInterfaces().SingleInstance();

        // remember that we are loaded into the context into the LogScannerEngine, so if you 
        // need file system access to where your plugin is located on disk, you need to 
        // use something like 
        // var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
