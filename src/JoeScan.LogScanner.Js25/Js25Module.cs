using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Js25
{
    public  class Js25Module : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Js25Adapter>().As<IScannerAdapter>();
            builder.Register(c => new ConfigurationBuilder<IJs25AdapterConfig>()
                .UseIniFile("Js25Adapter.ini")
                .Build()).SingleInstance();
        }

        
    }
}
