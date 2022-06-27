using Autofac;
using Autofac.Features.AttributeFilters;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LogScannerEngine>().AsSelf().SingleInstance();
        builder.RegisterType<SingleZoneLogAssembler>().As<ILogAssembler>().SingleInstance();
        builder.RegisterType<RawProfileValidator>().As<IRawProfileValidator>();
        builder.RegisterType<PieceNumberProvider>().As<IPieceNumberProvider>().SingleInstance();
        builder.RegisterType<RawLogArchiver>().As<ILogArchiver>().SingleInstance();

        builder.RegisterType<FlightsAndWindowFilter>().As<IFlightsAndWindowFilter>().SingleInstance();
        // the MuteNotifier is a no-op class. If a GUI or console app wants to use/display notifications 
        // coming out of the core module, it just needs to register it's own IUserNotifier instance after 
        // the CoreModule was registered
        builder.RegisterType<MuteNotifier>().As<IUserNotifier>();

        builder.RegisterType<DefaultConfigLocator>().As<IConfigLocator>();
        // this is a bit of a mouthful and almost java-like, but: 
        // whenever an instance of ICoreConfig is needed, we first resolve the IConfigLocator, which gives 
        // us the path to where our config files are stored. Then, we combine that path with the name for our main
        // core config file, let the Config.NET ConfigurationBuilder read it, and return it as an instance of ICoreConfig.
        // the upside is that we can easily change the location of where our config files live, in a centralized location 

        builder.Register(c => new ConfigurationBuilder<ICoreConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetConfigLocation(),"coreconfig.json"))
            .Build()).As<ICoreConfig>().SingleInstance();

    }
}
