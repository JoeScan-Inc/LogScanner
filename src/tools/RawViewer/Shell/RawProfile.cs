using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Models;

namespace RawViewer.Shell;

public class RawProfile 
{
    private readonly Profile p;

    public RawProfile(Profile p)
    {
        this.p = p;
    }
    public int Index { get; set; }
    public uint ScanHeadId => p.ScanHeadId;
    public Point2D[] Data => p.Data;
    public int NumPts => p.Data.Length;
    public ScanFlags ScanningFlags => p.ScanningFlags;
    public ushort LaserOnTimeUs => p.LaserOnTimeUs;
    public long EncoderValue => p.EncoderValues[0];
    public uint SequenceNumber => p.SequenceNumber;
    public ulong TimeStampNs => p.TimeStampNs;

}
