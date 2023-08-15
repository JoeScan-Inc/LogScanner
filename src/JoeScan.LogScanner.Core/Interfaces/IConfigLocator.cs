namespace JoeScan.LogScanner.Core.Interfaces;

public interface IConfigLocator
{
    string GetDefaultConfigLocation ();
    void OverrideDefaultConfigLocation (string path);

}
