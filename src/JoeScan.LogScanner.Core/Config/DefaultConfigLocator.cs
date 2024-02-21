using JoeScan.LogScanner.Core.Interfaces;
using NLog;

namespace JoeScan.LogScanner.Core.Config;

public class DefaultConfigLocator : IConfigLocator  
{
    public ILogger Logger { get; }
    
    public string ConfigLocation { get; private set; } = string.Empty;
    
    public DefaultConfigLocator(ILogger logger)
    {
        Logger = logger;
        
    }

    public string GetDefaultConfigLocation()
    {
        if (ConfigLocation != String.Empty)
        {
            return ConfigLocation;
        }
        // on a development machine, we need to have LOGSCANNER_ROOT set to the root of the LogScanner installation
        var logScannerRoot = Environment.GetEnvironmentVariable("LOGSCANNER_ROOT");
        if (!string.IsNullOrEmpty(logScannerRoot))
        {
            ConfigLocation = Path.Combine(logScannerRoot, "config", "Default");
            Logger.Info("Using LOGSCANNER_ROOT environment variable to locate config files: {0}", ConfigLocation);
        }
        else
        {
            // we are on a production machine, so we can use the default location, which is next to the binary folder
            // get the current executable's location
            ConfigLocation = Path.Combine(AppContext.BaseDirectory, "..", "config", "Default");
            Logger.Info("Using default config location: {0}", ConfigLocation);
        }
        return ConfigLocation;
    }

    public void OverrideDefaultConfigLocation(string path)
    {
        ConfigLocation = Path.IsPathRooted(path) ? path : Path.Combine(ConfigLocation, path);
        Logger.Info("Overriding config location to {0}", ConfigLocation);
    }

    public string GetSettingsLocation()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ExNovo", "LogScanner");
    }
}
