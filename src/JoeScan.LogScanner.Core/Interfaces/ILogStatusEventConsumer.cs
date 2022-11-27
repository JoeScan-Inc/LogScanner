namespace JoeScan.LogScanner.Core.Interfaces;

public interface ILogStatusEventConsumer
{
    void LogCollectionIdleStarted();
    void LogCollectionIdleEnded();

    void LogCollectionStarted();
    void LogCollectionAborted();
    void LogCollectionEnded();
    
}
