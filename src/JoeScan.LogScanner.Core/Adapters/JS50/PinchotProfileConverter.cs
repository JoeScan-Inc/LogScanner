using JoeScan.LogScanner.Core.Models;
using JoeScan.Pinchot;
using Point2D = JoeScan.LogScanner.Core.Geometry.Point2D;

namespace JoeScan.LogScanner.Core.Adapters.JS50;

public static class PinchotProfileConverter
{
    public static bool IsFakeEncoder => fakeEncoder;
    private static bool fakeEncoder = false;
    static PinchotProfileConverter()
    {
        var fakeEncoderEnvVar = Environment.GetEnvironmentVariable("LOGSCANNER_FAKE_ENCODER");
        if (fakeEncoderEnvVar != null)
        {
            fakeEncoder = true;
        }
    }
    public static Profile ToLogScannerProfile(this IProfile pProfile, ScanSystemUnits pUnits)
    {
        return new Profile
        {
            Units = pUnits == ScanSystemUnits.Inches ? UnitSystem.Inches: UnitSystem.Millimeters,
            Data = pProfile.GetValidXYPoints().Select(q => new Point2D(q.X, q.Y, q.Brightness)).ToArray(),
            ScanningFlags = ScanFlags.None,
            LaserIndex = (uint)pProfile.Laser,
            //TODO: cast from double?
            LaserOnTimeUs = (ushort)pProfile.LaserOnTimeUs,
            // if we want a fake encoder, we just use the sequence number as the encoder value
            EncoderValues = fakeEncoder ? 
                new Dictionary<uint, long>(){{0, pProfile.SequenceNumber}} :
                new Dictionary<uint, long>(){{0, 
                    // with Pinchot, if no encoder is connected, the dictionary is empty,
                    pProfile.EncoderValues.ContainsKey(0) ? pProfile.EncoderValues[0]:0L}},
            SequenceNumber = pProfile.SequenceNumber,
            //TODO: ulong cast ok?
            TimeStampNs = (ulong)pProfile.TimestampNs, 
            ScanHeadId = pProfile.ScanHeadID,
            Camera = (uint)pProfile.Camera,
            Inputs = InputFlags.None
        }; 
    }
}
