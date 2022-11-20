﻿using Config.Net;
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
    bool UseLogPresenceSignal { get;  }
    [DefaultValue(false)]
    bool StartScanInverted { get;  }
    [DefaultValue(3.0)]
    double MinProfileSpacing { get;  }
    [DefaultValue(6000.0)]
    double MaxLogLength { get;  }
    [DefaultValue(1000.0)]
    double MinLogLength { get;  }
    [DefaultValue(5)]
    int StopLogCount { get;  }
    [DefaultValue(20)]
    int StartLogCount { get;  }
    [DefaultValue(1.0)]
    double EncoderPulseInterval { get;  }
}

public interface ILogModelBuilderConfig
{
    [Option(Alias = "SectionInterval", DefaultValue = "100.0")]
    double SectionInterval { get; }
    [Option(DefaultValue = 0.0)]
    double DiameterEndOffset { get;  }
}

public interface IRawLogArchiverConfig
{
    string Location { get;  }
    bool Enabled { get; }
    

}

public interface IRawDumperConfig
{
    string RawDumpLocation { get;  }
}

public interface ISectionBuilderConfig
{
    [DefaultValue(true)]
    bool FilterOutliers { get;  }
    [DefaultValue(100)]
    int OutlierFilterMaxNumIterations { get;  }
    //TODO: Units?
    [DefaultValue(10.0)]
    double OutlierFilterMaxDistance { get;  }
    [DefaultValue(0.0)]
    double BarkAllowance { get;  }
    [DefaultValue(100)]

    int ModelPointCount { get;  }
    [DefaultValue(3.0)]
    double MaxOvality { get;  }
    [DefaultValue(1.0)]
    double MinimumLogDiameter { get;  }
    [DefaultValue(24.0)]
    double MaximumLogDiameter { get;  }
    [DefaultValue(300.0)]
    double LogMaximumPositionX { get;  }
    [DefaultValue(-100.0)]
    double LogMinimumPositionX { get;  }
    [DefaultValue(300.0)]
    double LogMaximumPositionY { get;  }
    [DefaultValue(0.0)]
    double LogMinimumPositionY { get;  }
    [DefaultValue(0.1)]
    double MaxFitError { get;  }

}
