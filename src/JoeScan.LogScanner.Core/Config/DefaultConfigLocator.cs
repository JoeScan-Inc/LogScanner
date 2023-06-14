using JoeScan.LogScanner.Core.Helpers.CommandLine;
using NLog;
using System.Reflection;

namespace JoeScan.LogScanner.Core.Config;

public class DefaultConfigLocator : IConfigLocator  
{
    public ILogger Logger { get; }
    private const string defaultProfileName = "Default";

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
    }

    public string GetDefaultConfigLocation()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "LogScanner","Profiles", ProfileName);
    }

    public string GetUserConfigLocation()
    {
        return GetDefaultConfigLocation();
    }
}
