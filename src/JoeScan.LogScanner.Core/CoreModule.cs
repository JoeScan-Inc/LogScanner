using Autofac;
using Autofac.Features.AttributeFilters;
using Config.Net;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LogScannerEngine>().AsSelf().SingleInstance();
        builder.RegisterType<SingleZoneLogAssembler>().As<ILogAssembler>();
        builder.RegisterType<RawProfileValidator>().As<IRawProfileValidator>();
        builder.RegisterType<PieceNumberProvider>().As<IPieceNumberProvider>().SingleInstance();

        // builder.RegisterType<PolygonFilter>().As<IFilterShape>().WithAttributeFiltering().SingleInstance();

        builder.RegisterType<FlightsAndWindowFilter>().As<IFlightsAndWindowFilter>().SingleInstance();
        builder.RegisterType<MuteNotifier>().As<IUserNotifier>();

        builder.Register(c => new ConfigurationBuilder<ICoreConfig>()
            .UseJsonFile("coreconfig.json")
            .Build()).As<ICoreConfig>().SingleInstance();

    }
}
