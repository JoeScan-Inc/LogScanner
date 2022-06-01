using Autofac;
using JoeScan.LogScanner.Core.Interfaces;
using Nini.Config;

namespace JoeScan.LogScanner.Js25
{
    public  class Js25Module : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Js25Adapter>().As<IScannerAdapter>();
            builder.Register(c => new IniConfigSource("Js25Adapter.ini")).Keyed<IConfigSource>("Js25Adapter.ini").SingleInstance();
        }

        
    }
}
