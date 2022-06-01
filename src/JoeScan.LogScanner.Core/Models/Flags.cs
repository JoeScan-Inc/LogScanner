namespace JoeScan.LogScanner.Core.Models;

/// <summary>
/// placeholder class for Js-25 flags
/// </summary>
[Flags]
public enum ScanFlags
{
    None = 0,
    Overrun = 1,
    InternalError = 2,
    SequenceError = 4
    // everything below is usable for Pinchot

}
