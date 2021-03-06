using JoeScan.LogScanner.Core.Models;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface ILogModelConsumer
{
    public void Consume(LogModel logModel);
}
