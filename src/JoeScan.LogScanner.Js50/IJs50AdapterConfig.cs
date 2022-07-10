using Config.Net;
using JoeScan.Pinchot;
using System.Security.Permissions;

namespace JoeScan.LogScanner.Js50;

public interface IJs50AdapterConfig
{
    public double ScanRate { get; set; }
    public DataFormat DataFormat { get; set; }
    public string Units { get; set; }
    public double EncoderPulseInterval { get; set; }

    IEnumerable<IJs50HeadConfig> ScanHeads { get;  }
}

public interface IJs50HeadConfig
{
    public uint Serial { get;  }
    public uint Id { get;  }
    public string Name { get;  }
    public double MinLaserOn { get;  }

    public double DefaultLaserOn { get;  }
    public double MaxLaserOn { get;  }
    public double ScanPhaseOffset { get;  }
    [Option(Alias = "Alignment.ShiftX")]
    public double AlignmentShiftX { get;  }
    [Option(Alias = "Alignment.ShiftY")]
    public double AlignmentShiftY { get;  }
    [Option(Alias = "Alignment.RollDegrees")]
    public double AlignmentRollDegrees { get;  }
    [Option (Alias ="Alignment.Orientation")]
    public ScanHeadOrientation AlignmentOrientation { get;  }
   
    [Option (Alias = "Window.Top")]
    public double WindowTop { get;  }
    [Option(Alias = "Window.Bottom")]
    public double WindowBottom{ get;  }
    [Option(Alias = "Window.Left")]
    public double WindowLeft { get;  }
    [Option(Alias = "Window.Right")]
    public double WindowRight { get;  }

}
