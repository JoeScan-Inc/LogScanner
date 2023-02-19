using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Models;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IScannerAdapter
{
    /// <summary>
    /// the Units in which the adapter delivers profiles. For Pinchot API v13 this is
    /// always Inches, for v14+ it can be Inches or Millimeters,
    /// depending on how the adapter was configured. For JS-20/25 models it can be Inches or Millimeters, depending
    /// on the settings in param.dat.
    /// </summary>
    UnitSystem Units { get; }
    BufferBlock<Profile> AvailableProfiles { get; }
    bool IsRunning { get; }
    /// <summary>
    /// causes the adapter to (re)configure. Needs to be done before scanning
    /// can be started. 
    /// </summary>
    /// <returns></returns>
    Task<bool> ConfigureAsync();
    bool IsConfigured { get; }
    Task<bool> StartAsync();
    void Stop();

    string Name { get; }

    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;

    // we use this flag to indicate that it is replayed or synthetic data, 
    // mostly to avoid that the raw dumper fills up the disk with garbage
    public bool IsReplay { get; }

    public uint VersionMajor { get; }
    public uint VersionMinor { get; }
    public uint VersionPatch { get; }

    public Guid Id { get; }

}
