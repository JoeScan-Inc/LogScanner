using Autofac;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Js50.Config;

namespace JoeScan.LogScanner.Js50;

public class Js50Module : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Js50Adapter>().As<IScannerAdapter>();


    }
}
