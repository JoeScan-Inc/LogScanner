using Autofac;
using Autofac.Features.AttributeFilters;
using Config.Net;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using McMaster.NETCore.Plugins;
using System.Reflection;
using Module = Autofac.Module;

namespace JoeScan.LogScanner.Core;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LogScannerEngine>().AsSelf().SingleInstance();
        builder.RegisterType<SingleZoneLogAssembler>().As<ILogAssembler>().SingleInstance();
        builder.RegisterType<RawProfileValidator>().As<IRawProfileValidator>();
        builder.RegisterType<PieceNumberProvider>().As<IPieceNumberProvider>().SingleInstance().OnRelease(instance=>instance.Dispose());
        builder.RegisterType<RawLogArchiver>().As<ILogArchiver>().SingleInstance();

        builder.RegisterType<FlightsAndWindowFilter>().As<IFlightsAndWindowFilter>().SingleInstance();
       
        builder.RegisterType<LogModelBuilder>().AsSelf().SingleInstance();
        builder.RegisterType<LogSectionBuilder>().AsSelf().SingleInstance();


        builder.RegisterType<DefaultConfigLocator>().As<IConfigLocator>();
        // this is a bit of a mouthful and almost java-like, but: 
        // whenever an instance of ICoreConfig is needed, we first resolve the IConfigLocator, which gives 
        // us the path to where our config files are stored. Then, we combine that path with the name for our main
        // core config file, let the Config.NET ConfigurationBuilder read it, and return it as an instance of ICoreConfig.
        // the upside is that we can easily change the location of where our config files live, in a centralized location.
        // 
        // Giving the Registration two files creates a merge. The idea here is that coreconfig contains 
        // factory defaults, and will be updated from the contents of the git repo.
        // The version with the _user suffix can be used to selectively override values specific for that site
        // see here for details: https://github.com/aloneguid/config#using-multiple-sources

        builder.Register(c => new ConfigurationBuilder<ICoreConfig>()
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetUserConfigLocation(), "coreconfig_user.json"))
            .UseJsonFile(Path.Combine(c.Resolve<IConfigLocator>().GetDefaultConfigLocation(),"coreconfig.json"))
            .Build()).As<ICoreConfig>().SingleInstance();
        

       RegisterPlugins(builder, GetPluginLoaders("adapters"));
       RegisterPlugins(builder, GetPluginLoaders("extensions"));

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

    private static List<PluginLoader> GetPluginLoaders(string subFolder)
    {
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
        if (Path.Exists(Path.Combine(path!,subFolder)))
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
