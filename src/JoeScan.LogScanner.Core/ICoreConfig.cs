using Config.Net;
using System.ComponentModel;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core;

public interface ICoreConfig
{
    UnitSystem Units { get; }
    ISingleZoneLogAssemblerConfig SingleZoneLogAssemblerConfig { get; }
    [Option(Alias = "LogModelBuilder")]
    ILogModelBuilderConfig LogModelBuilderConfig { get; }

    [Option(Alias="LogSectionBuilder")]
    ISectionBuilderConfig SectionBuilderConfig { get; }
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

public interface ILogModelBuilderConfig
{
    [Option(Alias = "SectionInterval", DefaultValue = "100.0")]
    double SectionInterval { get; set; }
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

public interface ISectionBuilderConfig
{
    [DefaultValue(true)]
    bool FilterOutliers { get; set; }
    [DefaultValue(100)]
    int OutlierFilterMaxNumIterations { get; set; }
    //TODO: Units?
    [DefaultValue(10.0)]
    double OutlierFilterMaxDistance { get; set; }
    [DefaultValue(0.0)]
    double BarkAllowance { get; set; }
    [DefaultValue(100)]
    int ModelPointCount { get; set; }
    [DefaultValue(3.0)]
    double MaxOvality { get; set; }
    [DefaultValue(60.0)]
    double MinimumLogDiameter { get; set; }
    [DefaultValue(600.0)]
    double MaximumLogDiameter { get; set; }
    [DefaultValue(300.0)]
    double LogMaximumPositionX { get; set; }
    [DefaultValue(0.0)]
    double LogMinimumPositionX { get; set; }
    [DefaultValue(300.0)]
    double LogMaximumPositionY { get; set; }
    [DefaultValue(-300.0)]
    double LogMinimumPositionY { get; set; }

}
