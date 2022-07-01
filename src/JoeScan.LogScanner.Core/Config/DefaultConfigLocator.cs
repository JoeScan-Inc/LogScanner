using System.Reflection;

namespace JoeScan.LogScanner.Core.Config;

public class DefaultConfigLocator : IConfigLocator  
{
  

    public string GetConfigLocation()
    {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
    }
}
