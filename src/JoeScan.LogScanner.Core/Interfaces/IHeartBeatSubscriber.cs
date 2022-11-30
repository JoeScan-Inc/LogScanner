namespace JoeScan.LogScanner.Core.Interfaces;

public interface IHeartBeatSubscriber
{
    TimeSpan RequestedInterval { get; }
    void Callback(bool isRunning);
}
