using JoeScan.LogScanner.Core.Helpers.CommandLine;
using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.Reflection;

namespace JoeScan.LogScanner.Core.Config;

public class DefaultConfigLocator : IConfigLocator  
{
    public ILogger Logger { get; }
    private const string defaultProfileName = "Default";
    private bool standalone = true;
    public string ProfileName { get; private set; } = defaultProfileName;

    public DefaultConfigLocator(ILogger logger)
    {
        Logger = logger;
        var parsedOptions =
            CommandLineParser.ParseOptions(Environment.GetCommandLineArgs().Skip(1).ToArray());
        if (parsedOptions != null && parsedOptions.Any(q => q.Name.ToLower() == "profile"))
        {
            var option = parsedOptions.FirstOrDefault(q => q.Name.ToLower() == "profile");
            if (option != null && !String.IsNullOrEmpty(option.Value))
            {
                ProfileName = option.Value;
                
            }
        }
        Logger.Info($"Using Profile \"{ProfileName}\"");
        var envVars = Environment.GetEnvironmentVariables();
        if (envVars.Contains("VisualStudioVersion") || envVars.Contains("IDEA_INITIAL_DIRECTORY"))
        {
            Logger.Info("Running within IDE");
            standalone = false;
        }
        else
        {
            Logger.Info("Running standalone");
        }
    }

    public string GetDefaultConfigLocation()
    {
        // get the path of the executing assembly
        var executable = Environment.ProcessPath;
        var path = Path.GetDirectoryName(executable);
        if (standalone)
        {
            // running standalone, so we need to go up a directory and then into config 
            // and then either into "Default" or the profile name
            return Path.Combine(path!, "..", "config", ProfileName);
        }
        else
        {
            // running within IDE, so we need to go up four directories and then into config
            // *.dll is in bin/Debug/net7.0
            
            return Path.Combine(path!, "../../../../..", "config", ProfileName);
        }
    }

    public string GetUserConfigLocation()
    {
        return GetDefaultConfigLocation();
    }
}
