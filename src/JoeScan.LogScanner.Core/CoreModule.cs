using Autofac;
using Autofac.Extras.NLog;
using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using McMaster.NETCore.Plugins;
using Nini.Config;
using NLog;
using System.Reflection;
using Module = Autofac.Module;

namespace JoeScan.LogScanner.Core;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<NLogModule>();

        builder.RegisterType<LogScannerEngine>().AsSelf().SingleInstance();
        builder.RegisterType<SingleZoneLogAssembler>().As<ILogAssembler>().SingleInstance();
        builder.RegisterType<RawProfileValidator>().As<IRawProfileValidator>();
        builder.RegisterType<PieceNumberProvider>().As<IPieceNumberProvider>().SingleInstance()
            .OnRelease(instance => instance.Dispose());
        builder.RegisterType<RawLogArchiver>().As<ILogArchiver>().SingleInstance();

        builder.RegisterType<FlightsAndWindowFilter>().As<IFlightsAndWindowFilter>().SingleInstance();

        builder.RegisterType<LogModelBuilder>().AsSelf().SingleInstance();
        builder.RegisterType<LogSectionBuilder>().AsSelf().SingleInstance();
        builder.RegisterType<RawProfileDumper>().AsSelf().SingleInstance();


        builder.RegisterType<DefaultConfigLocator>().As<IConfigLocator>();

        // we store all configs in a single location, 
        // independent of the binaries location.
        // the following code resolves the location at runtime by 
        // first resolving an instance of the DefaultConfigLocator 
        // and then asking it for the location of configfiles (DefaultConfigLocation)
        // the IniConfigSource is used multiple times, as all core components 
        // use the same file, only different sections.

        // This allows us to be flexible with where the files live.
        builder.Register(c =>
        {
            var configLocator = c.Resolve<IConfigLocator>();
            var configLocation = configLocator.GetDefaultConfigLocation();
            return new CoreConfig(new IniConfigSource(Path.Combine(configLocation, "core.ini")).Configs["Core"]);
        }).AsSelf();

        builder.Register(c =>
        {
            var configLocator = c.Resolve<IConfigLocator>();
            var configLocation = configLocator.GetDefaultConfigLocation();
            return new SingleZoneLogAssemblerConfig(
                new IniConfigSource(Path.Combine(configLocation, "core.ini")).Configs["SingleZoneLogAssembler"]);
        });
        ;
        builder.Register(c =>
            {
                var configLocator = c.Resolve<IConfigLocator>();
                var configLocation = configLocator.GetDefaultConfigLocation();
                return new LogModelBuilderConfig(
                    new IniConfigSource(Path.Combine(configLocation, "core.ini")).Configs["LogModelBuilder"]);
            }
        );

        builder.Register(c =>
            {
                var configLocator = c.Resolve<IConfigLocator>();
                var configLocation = configLocator.GetDefaultConfigLocation();
                return new RawLogArchiverConfig(
                    new IniConfigSource(Path.Combine(configLocation, "core.ini")).Configs["RawLogArchiver"]);
            }
        );
        builder.Register(c =>
            {
                var configLocator = c.Resolve<IConfigLocator>();
                var configLocation = configLocator.GetDefaultConfigLocation();
                return new RawDumperConfig(
                    new IniConfigSource(Path.Combine(configLocation, "core.ini")).Configs["RawDumper"]);
            }
        );
        builder.Register(c =>
            {
                var configLocator = c.Resolve<IConfigLocator>();
                var configLocation = configLocator.GetDefaultConfigLocation();
                return new SectionBuilderConfig(
                    new IniConfigSource(Path.Combine(configLocation, "core.ini")).Configs["SectionBuilder"]);
            }
        );

        // the sensor adapter plugins (JS-50Adapter, Replay, Synthetic etc.) all live in 
        // a separate folder. 
        RegisterPlugins(builder, GetPluginLoaders("adapters"));
        // extension plugins (things that implement ILogModelConsumerPlugin) live here.
        RegisterPlugins(builder, GetPluginLoaders("extensions"));
    }


    private static List<PluginLoader> GetPluginLoaders(string subFolder)
    {
        // plugin loaders are a concept from the .NET Core Plugins library
        // https://github.com/natemcmaster/DotNetCorePlugins
        var loaders = new List<PluginLoader>();
        // create plugin loaders
        var pluginsDir = GetPluginsPath(subFolder);
        if (pluginsDir != null)
        {
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName = Path.GetFileName(dir);
                var pluginDll = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(pluginDll))
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        pluginDll,
                        sharedTypes: new[] { typeof(IPluginFactory), typeof(ContainerBuilder) });
                    loaders.Add(loader);
                }
            }
        }

        return loaders;
    }

    private static void RegisterPlugins(ContainerBuilder builder, List<PluginLoader> loaders)
    {
        // Create an instance of plugin types
        foreach (var loader in loaders)
        {
            var assembly = loader.LoadDefaultAssembly();
            var types = assembly.GetTypes();
            foreach (var t in types)
            {
                if (typeof(IPluginFactory).IsAssignableFrom(t) && !t.IsAbstract)
                {
                    var plugin = Activator.CreateInstance(t) as IPluginFactory;
                    plugin?.Configure(builder);
                }
            }
        }
    }


    private static string? GetPluginsPath(string subFolder)
    {
        //TODO: write a guide to implementing plugins
        // adapter and vendor modules to be registered from assemblies
        // for a deployed system, the location of the adapter plugins (implementing IScannerAdapter) is in a folder named "adapters" ,
        // for other extensions implementing the ILogModelConsumer interface, the folder is "extensions",
        // next to the executable,
        // but during development, we rely on a relative path. Hopefully, one or the other exists
        string? adapterpath = null;
        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        if (Path.Exists(Path.Combine(path!, subFolder)))
        {
            // we are in a deployed system, the adapters folder is right next to the executable
            adapterpath = Path.Combine(path!, subFolder);
        }
        else
        {
            var p = Path.GetFullPath(Path.Combine(path!, @"..\..\..\..\..", "bin", subFolder));
            if (Path.Exists(p))
            {
                adapterpath = p;
            }
        }

        return adapterpath;
    }
}
