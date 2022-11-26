using JoeScan.LogScanner.Core.Models;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface ILogModelConsumerPlugin : IDisposable
{
    public string PluginName { get; }
    public int VersionMajor { get; }
    public int VersionMinor { get; }
    public int VersionPatch { get; }
    public Guid Id { get; }
    void Initialize();
    bool IsInitialized { get; }
    void Cleanup();
    void Consume(LogModel logModel);


}
