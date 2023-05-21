using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace CylinderFitter;
public class CylinderFitterModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CylinderFitterPlugin>().As<ILogModelConsumerPlugin>().As<IDisposable>().SingleInstance();
    }
}
