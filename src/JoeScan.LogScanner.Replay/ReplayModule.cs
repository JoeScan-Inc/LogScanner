using Autofac;
using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Interfaces;
using Nini.Config;

namespace JoeScan.LogScanner.Replay;
public class ReplayModule : Module 
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ReplayAdapter>().As<IScannerAdapter>().WithAttributeFiltering();
        builder.Register(c => new IniConfigSource("ReplayAdapter.ini"))
            .Keyed<IConfigSource>("ReplayAdapter.ini").SingleInstance();
    }
}
