using System.ComponentModel;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core;

public interface ICoreConfig
{
    UnitSystem Units { get; }
    ISingleZoneLogAssemblerConfig SingleZoneLogAssemblerConfig { get; }

    IRawLogArchiverConfig RawLogArchiverConfig { get; }
    IRawDumperConfig RawDumperConfig { get; }

}

public interface ISingleZoneLogAssemblerConfig
{
    // for DefaultValues, use either string or see here:
    // https://github.com/aloneguid/config#default-values

    [DefaultValue(false)]
    bool UseLogPresenceSignal { get; set; }
    [DefaultValue(false)]
    bool StartScanInverted { get; set; }
    [DefaultValue(3.0)]
    double MinProfileSpacing { get; set; }
    [DefaultValue(6000.0)]
    double MaxLogLength { get; set; }
    [DefaultValue(1000.0)]
    double MinLogLength { get; set; }
    [DefaultValue(5)]
    int StopLogCount { get; set; }
    [DefaultValue(20)]
    int StartLogCount { get; set; }
    [DefaultValue(1.0)]
    double EncoderPulseInterval { get; set; }
}

public interface IRawLogArchiverConfig
{
    string Location { get; set; }
  
    //TODO: delete oldest, set max number of archived logs

}

public interface IRawDumperConfig
{
    string RawDumpLocation { get; set; }
}
