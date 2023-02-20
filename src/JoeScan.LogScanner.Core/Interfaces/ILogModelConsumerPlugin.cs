using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Models;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface ILogModelConsumerPlugin : IPlugin, IDisposable
{
   
    void Initialize();
    bool IsInitialized { get; }
    void Cleanup();
    void Consume(LogModelResult logModel);

    

}
