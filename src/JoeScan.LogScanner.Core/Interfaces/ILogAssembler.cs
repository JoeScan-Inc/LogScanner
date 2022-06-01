using JoeScan.LogScanner.Core.Models;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Interfaces;

/// <summary>
/// describes the interface for classes that take individual profiles and
/// do something with them, e.g. display or assemble into something
/// </summary>
public interface ILogAssembler
{


    public void AddProfile(Profile profile);
    public BufferBlock<RawLog> RawLogs { get; }

}
