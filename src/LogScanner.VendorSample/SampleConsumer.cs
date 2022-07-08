using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using System.Diagnostics;

namespace LogScanner.VendorSample;
public class SampleConsumer : ILogModelConsumer
{
    public void Consume(LogModel logModel)
    {
        // do whatever with model here. Ideally, this should be quick, as it is executed
        // within the LogScannerEngine, so the recommended approach is a producer-consumer
        // pattern that just stores the model and processes it in it's own task
    }
}
