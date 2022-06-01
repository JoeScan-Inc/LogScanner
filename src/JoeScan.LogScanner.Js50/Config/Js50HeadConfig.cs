using JoeScan.Pinchot;
using Newtonsoft.Json;

namespace JoeScan.LogScanner.Js50.Config;

public class Js50HeadConfig
{
    public uint Serial { get; set; } 
    public uint Id { get; set; }
    public string Name { get; set; }
    public double MinLaserOn { get; set; }

    public double DefaultLaserOn { get; set; }
    public double MaxLaserOn { get; set; }
    public double ScanPhaseOffset { get; set; }
    public AlignmentValues Alignment { get; set; }
    public WindowValues Window { get; set; }
    public class AlignmentValues
    {
        public double RollDegrees { get; set; }
        public double ShiftX { get; set; }
        public double ShiftY { get; set; }
        public ScanHeadOrientation Orientation { get; set; }
    }

    public class WindowValues
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
    }
}
