using CellWinNet;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;

namespace PlcConnector;
public class PlcConnectorPlugin : ILogModelConsumerPlugin
{
    public string PluginName { get; } = "PlcConnector";
    public int VersionMajor { get; } = 1;
    public int VersionMinor { get; } = 0;
    public int VersionPatch { get; } = 0;
    public Guid Id { get; } = Guid.Parse("{2AB395EE-5181-4B4F-93F2-0DB8CAF36894}");
    public void Initialize()
    {
        CellWinConfiguration.Initialize();
    }

    public bool IsInitialized { get; }

    public void Cleanup()
    {
        
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
}
