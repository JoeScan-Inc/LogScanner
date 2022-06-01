namespace JoeScan.LogScanner.Core.Models;

/// <summary>
/// placeholder to adapt the InputFlags value from JS-25 API to a generic
/// flag type, as Pinchot does not have such capabilities
/// </summary>
[Flags]
public enum InputFlags
{
    None = 0,
    EncoderB = 1,
    EncoderA = 2,
    StartScan = 4
    // everything below is usable for Pinchot

}
