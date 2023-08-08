namespace JoeScan.LogScanner.Core.Interfaces;

public interface IConfigLocator
{
    string GetDefaultConfigLocation ();
    string GetUserConfigLocation ();

    string ProfileName { get; }
}
