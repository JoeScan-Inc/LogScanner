using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System.Diagnostics;

namespace LogScanner.VendorSample;
public class SampleConsumer : ILogModelConsumerPlugin, IHeartBeatSubscriber
{
    public ILogger Logger { get; }
    public string PluginName { get; } = "SampleConsumerPlugin";
    public int VersionMajor { get; } = 1;
    public int VersionMinor { get; } = 0;
    public int VersionPatch { get; } = 0;
    public Guid Id { get; } = Guid.Parse("{CB14AAAE-09E2-4C72-A89F-96FBFFA77DEC}");
    public void Initialize()
    {
        IsInitialized = true;
    }

    public bool IsInitialized { get; private set; }

    public void Cleanup()
    {
       // nothing in the sample
    }

    public void Consume(LogModel logModel)
    {
        // do whatever with model here. Ideally, this should be quick, as it is executed
        // within the LogScannerEngine, so the recommended approach is a producer-consumer
        // pattern that just stores the model and processes it in it's own task
    }

    public void Dispose()
    {

    }

    public SampleConsumer()
    {
        Logger = LogManager.GetCurrentClassLogger();
    }

    public TimeSpan RequestedInterval { get; } = TimeSpan.FromSeconds(5);
    public void Callback(bool isRunning)
    {
        Logger.Trace(isRunning?"Adapter is up and running.":"Adapter not running.");
    }
}
