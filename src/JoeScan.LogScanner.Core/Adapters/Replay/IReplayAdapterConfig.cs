using Config.Net;

namespace JoeScan.LogScanner.Core.Adapters.Replay;

public interface IReplayAdapterConfig
{
    [Option(Alias = "Replay.File")]
    string File { get; }
}
