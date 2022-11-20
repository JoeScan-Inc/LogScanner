using Config.Net;

namespace JoeScan.LogScanner.Replay;

public interface IReplayAdapterConfig
{
    [Option(Alias = "Replay.File")]
    string File { get; }
}
