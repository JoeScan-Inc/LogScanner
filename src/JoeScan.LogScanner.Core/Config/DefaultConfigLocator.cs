using NLog;
using System.Reflection;

namespace JoeScan.LogScanner.Core.Config;

public class DefaultConfigLocator : IConfigLocator  
{
    public ILogger Logger { get; }

    public DefaultConfigLocator(ILogger logger)
    {
        Logger = logger;
    }

    public string GetDefaultConfigLocation()
    {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        //Logger.Debug($"Code base of executing assembly is: {codeBase}");
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        //Logger.Debug($"Path for code base is: {path}");
        // deployedPath exists on a customer machine, where the config subdirectory
        // is in the same directory as the executables
        string deployedPath = Path.Combine(Path.GetDirectoryName(path), "..","config");
       // Logger.Debug($"Search path for deployed version is: {deployedPath}");
        if (Directory.Exists(deployedPath))
        {
            Logger.Debug($"Search for deployed version successful.");
            return deployedPath;
        }
       // Logger.Debug($"Search for deployed version unsuccessful - looking for configs in repository instead.");
        // we're in a Visual Studio environment
        return "..\\..\\..\\..\\..\\config";
    }

    public string GetUserConfigLocation()
    {
        // TODO: maybe check for environment variable here and override
        return GetDefaultConfigLocation();

    }
}
