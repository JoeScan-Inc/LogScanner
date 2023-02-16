using Autofac;
using Config.Net;
using JoeScan.LogScanner.Core.Interfaces;
using System.Reflection;
using Module = Autofac.Module;

namespace ProblemLogArchiver;

public class ProblemLogArchiverModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ProblemLogArchiver>().As<ILogModelConsumerPlugin>().As<IDisposable>();
        builder.Register(c =>
        {
            // since we're loaded into the main engine's application context,
            // we won't find the config file next to the plugin dll. 
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return new ConfigurationBuilder<IProblemLogArchiverConfig>()
                .UseJsonFile(Path.Combine(assemblyPath!, "ProblemLogArchiverConfig.json"))
                .Build();
        }).As<IProblemLogArchiverConfig>().SingleInstance();
    }
}
