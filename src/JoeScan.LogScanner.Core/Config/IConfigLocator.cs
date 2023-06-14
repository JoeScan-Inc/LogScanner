namespace JoeScan.LogScanner.Core.Config;

public interface IConfigLocator
{
    string GetDefaultConfigLocation ();
    string GetUserConfigLocation ();

    string ProfileName { get; }
}
