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
            EncoderValues = pProfile.EncoderValues.ToDictionary(q=>(uint)q.Key,q=>q.Value),
            SequenceNumber = 0, // not available in Pinchot 13
            //TODO: ulong cast ok?
            TimeStampNs = (ulong)pProfile.Timestamp, 
            ScanHeadId = pProfile.ScanHeadID,
            Camera = (uint)pProfile.Camera,
            Inputs = InputFlags.None
        };
    }
}
