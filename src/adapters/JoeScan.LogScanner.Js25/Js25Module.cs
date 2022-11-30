using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Config.Net;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Js25
{
    public  class Js25Module : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Js25Adapter>().As<IScannerAdapter>();
            
        }

        
    }
}
