using Autofac;
using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using Nini.Config;

namespace JoeScan.LogScanner.Core;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LogScannerEngine>().AsSelf().WithAttributeFiltering().SingleInstance();
        builder.RegisterType<SingleZoneLogAssembler>().As<ILogAssembler>().WithAttributeFiltering();
        builder.RegisterType<SingleZoneLogAssemblerConfig>().AsSelf().WithAttributeFiltering();
        builder.RegisterType<RawProfileValidator>().As<IRawProfileValidator>().WithAttributeFiltering();
        builder.RegisterType<PieceNumberProvider>().As<IPieceNumberProvider>().SingleInstance().WithAttributeFiltering();

        // builder.RegisterType<PolygonFilter>().As<IFilterShape>().WithAttributeFiltering().SingleInstance();

        builder.Register(c => new IniConfigSource("Core.ini")).Keyed<IConfigSource>("Core.ini").SingleInstance();
        builder.RegisterType<FlightsAndWindowFilter>().As<IFlightsAndWindowFilter>().SingleInstance();
        builder.RegisterType<MuteNotifier>().As<IUserNotifier>();

    }
}
