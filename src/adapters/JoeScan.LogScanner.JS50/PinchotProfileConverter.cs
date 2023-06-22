using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Js50;

public static class PinchotProfileConverter
{
    public static Profile ToLogScannerProfile(this Pinchot.IProfile pProfile, Pinchot.ScanSystemUnits units)
    {
        return Profile.Build(
            units == Pinchot.ScanSystemUnits.Inches ? UnitSystem.Inches : UnitSystem.Millimeters,
            pProfile.ScanHeadID,
            (uint)pProfile.Laser,
            (uint)pProfile.Camera,
            pProfile.LaserOnTimeUs,
            pProfile.EncoderValues.Count > 0 ? pProfile.EncoderValues[0] : 0,
            pProfile.SequenceNumber,
            pProfile.TimestampNs,
            (ScanFlags)pProfile.Flags,
            InputFlags.None,
            pProfile.GetValidXYPoints().Select(p => new Point2D(p.X, p.Y, p.Brightness)).ToArray()
        );
    }
}
