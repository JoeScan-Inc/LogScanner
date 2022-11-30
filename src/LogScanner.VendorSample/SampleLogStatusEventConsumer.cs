using JoeScan.LogScanner.Core.Interfaces;

namespace LogScanner.VendorSample;

public class SampleLogStatusEventConsumer : ILogStatusEventConsumer, IDisposable
{
    public void LogCollectionIdleStarted()
    {
        
    }

    public void LogCollectionIdleEnded()
    {
    }

    public void LogCollectionStarted()
    {
    }

    public void LogCollectionAborted()
    {
    }

    public void LogCollectionEnded()
    {
     
    }

    public void Dispose()
    {

    }
}
