using Autofac;
using JoeScan.LogScanner.Core.Interfaces;

namespace LogScanner.VendorSample;

public class VendorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SampleConsumer>().As<ILogModelConsumerPlugin>().As<IDisposable>();
        builder.RegisterType<SampleLogStatusEventConsumer>()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}
