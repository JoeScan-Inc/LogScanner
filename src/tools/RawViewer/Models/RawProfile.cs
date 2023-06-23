using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Models;
using System.Collections.Generic;

namespace RawViewer.Models;

public class RawProfile
{
    private readonly Profile p;

    public RawProfile(Profile p)
    {
        this.p = p;
    }

    public Profile Profile => p;
    public int Index { get; set; }
    public uint ScanHeadId => p.ScanHeadId;
    public IEnumerable<Point2D> Data => p.GetValidPoints();
    public int NumPts => p.NumValidPoints;
    public ScanFlags ScanningFlags => p.ScanningFlags;
    public ushort LaserOnTimeUs => p.LaserOnTimeUs;
    public long EncoderValue => p.Encoder;
    public uint SequenceNumber => p.SequenceNumber;
    public ulong TimeStampNs => p.TimeStampNs;

    public ulong ReducedTimeStampNs { get; set; }
    public long ReducedEncoder { get; set; }
    public uint Camera => p.CameraIndex;
    public double ReducedTimeStampUs => ReducedTimeStampNs / 1000.0 ;

}
