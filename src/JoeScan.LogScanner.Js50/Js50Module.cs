using Autofac;
using JoeScan.LogScanner.Core;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Js50.Config;

namespace JoeScan.LogScanner.Js50;

public class Js50Module : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<CoreModule>();
        builder.RegisterType<Js50Adapter>().As<IScannerAdapter>();


    }
}
