using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Models;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IScannerAdapter
{
    /// <summary>
    /// the Units in which the adapter delivers profiles. For Pinchot API v13 this is
    /// always Inches, but for later versions of the API we can choose to use Millimeters.
    /// </summary>
    UnitSystem Units { get; }
    BufferBlock<Profile> AvailableProfiles { get; }
    bool IsRunning { get; }
    /// <summary>
    /// causes the adapter to (re)configure. Needs to be done before scanning
    /// can be started. 
    /// </summary>
    /// <returns></returns>
    void Configure();
    bool IsConfigured { get; }
    void Start();
    Task StartAsync();
    void Stop();
    Task StopAsync();

    string Name { get; }

    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;

}
