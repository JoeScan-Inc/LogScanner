using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Js50;

public static class PinchotProfileConverter
{
    public static LogScanner.Core.Models.Profile ToLogScannerProfile(this Pinchot.Profile pProfile)
    {
        return new LogScanner.Core.Models.Profile
        {
            Data = pProfile.GetValidXYPoints().Select(q => new Point2D(q.X, q.Y, q.Brightness)).ToArray(),
            ScanningFlags = ScanFlags.None,
            LaserIndex = (uint)pProfile.Laser,
            //TODO: cast from double?
            LaserOnTimeUs = (ushort)pProfile.LaserOnTime,
            EncoderValues = new Dictionary<uint, long>() { { 0, pProfile.EncoderValues[0] } },
            SequenceNumber = 0, // not available in Pinchot 13
            //TODO: ulong cast ok?
            TimeStampNs = (ulong)pProfile.Timestamp, 
            ScanHeadId = pProfile.ScanHeadID,
            Camera = (uint)pProfile.Camera,
            Inputs = InputFlags.None
        };
    }
}
