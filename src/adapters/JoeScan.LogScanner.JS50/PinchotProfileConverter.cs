using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Js50;

public static class PinchotProfileConverter
{
    public static LogScanner.Core.Models.Profile ToLogScannerProfile(this Pinchot.IProfile pProfile)
    {
        return new LogScanner.Core.Models.Profile
        {
            Units = UnitSystem.Inches,
            Data = pProfile.GetValidXYPoints().Select(q => new Point2D(q.X, q.Y, q.Brightness)).ToArray(),
            ScanningFlags = ScanFlags.None,
            LaserIndex = (uint)pProfile.Laser,
            //TODO: cast from double?
            LaserOnTimeUs = (ushort)pProfile.LaserOnTimeUs,
            // with Pinchot, if no encoder is connected, the dictionary is empty,
            // we just add 0 as the value for encoder 0 if that is the case
            EncoderValues = new Dictionary<uint, long>(){{0, pProfile.EncoderValues.ContainsKey(0) ? pProfile.EncoderValues[0]:0L}},
            SequenceNumber = 0, // not available in Pinchot 13
            //TODO: ulong cast ok?
            TimeStampNs = (ulong)pProfile.TimestampNs, 
            ScanHeadId = pProfile.ScanHeadID,
            Camera = (uint)pProfile.Camera,
            Inputs = InputFlags.None
        }; 
    }
}
