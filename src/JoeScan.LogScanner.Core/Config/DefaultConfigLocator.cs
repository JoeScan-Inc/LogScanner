using JoeScan.LogScanner.Core.Helpers.CommandLine;
using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.Reflection;

namespace JoeScan.LogScanner.Core.Config;

public class DefaultConfigLocator : IConfigLocator  
{
    public ILogger Logger { get; }
    
    public string ConfigLocation { get; private set; } = string.Empty;
    
    public DefaultConfigLocator(ILogger logger)
    {
        Logger = logger;
        
        var envVars = Environment.GetEnvironmentVariables();
        var executable = Environment.ProcessPath;
        var path = Path.GetDirectoryName(executable);
        
        if (envVars.Contains("VisualStudioVersion") || envVars.Contains("IDEA_INITIAL_DIRECTORY"))
        {
            Logger.Info("Running within IDE");
            ConfigLocation =  Path.Combine(path!, "../../../../..", "config", "Default");
            Logger.Info("Running within IDE, config location is {0}", ConfigLocation);
        }
        else
        {
            ConfigLocation = Path.Combine(path!, "..", "config", "Default");
            Logger.Info("Running standalone, config location is {0}", ConfigLocation);
        }
    }

    public string GetDefaultConfigLocation()
    {
        // get the path of the executing assembly
        return ConfigLocation;
    }

    public void OverrideDefaultConfigLocation(string path)
    {
        if (Path.IsPathRooted(path))
            ConfigLocation = path;
        else
        {
            ConfigLocation = Path.Combine(ConfigLocation, path);
        }
        Logger.Info("Overriding config location to {0}", ConfigLocation);
    }

   

    
}
