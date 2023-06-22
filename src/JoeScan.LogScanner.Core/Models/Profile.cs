using JoeScan.LogScanner.Core.Geometry;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JoeScan.LogScanner.Core.Models;

/// <summary>
/// This is a generic profile, representing a single scan. It is assumed to be in
/// mill coordinate system already.
/// </summary>
[DebuggerDisplay("Id = {ScanHeadId}, NumPoints = {Data.Length}, Enc = {EncoderValues[0]}")]
public record Profile
{
    /// <summary>
    /// Private Constructor
    /// </summary>
    private Profile()
    {
        // just to keep the nullability checker happy
        data = Array.Empty<Point2D>();
    }

    public static Profile Build(
        UnitSystem unitSystem,
        uint scanHeadId,
        uint laserIndex,
        uint cameraIndex,
        ushort laserOnTimeUs,
        long encoder,
        uint sequenceNumber,
        ulong timeStampNs,
        ScanFlags flags,
        InputFlags inputs,
        Point2D[] pts)
    {
        var p = new Profile
        {
            Units = unitSystem,
            ScanHeadId = scanHeadId,
            LaserIndex = laserIndex,
            CameraIndex = cameraIndex,
            LaserOnTimeUs = laserOnTimeUs,
            Encoder = encoder,
            SequenceNumber = sequenceNumber,
            TimeStampNs = timeStampNs,
            ScanningFlags = flags,
            Inputs = inputs,
            data = new Point2D[pts.Length]
        };
        Array.Copy(pts, p.data, pts.Length);
        p.BoundingBox = Geometry.BoundingBox.Get(p.Data);
        return p;
    }

    public UnitSystem Units { get; init; }

    /// <summary>
    /// encapsulates the JS-25 flags value, for Pinchot this is empty as the profile there does not
    /// have any
    /// </summary>
    public ScanFlags ScanningFlags { get; init; }

    /// <summary>
    /// LaserIndex is always 1 for JS-25 and corresponds to the enum index for JS-50 Pinchot, in
    /// both cases 0 is invalid. Also note that this is uint and not int as in JS-25 interface
    /// </summary>
    public uint LaserIndex { get; init; }

    /// <summary>
    /// LaserOn time in microseconds. 
    /// </summary>
    public ushort LaserOnTimeUs { get; init; }

    /// <summary>
    /// corresponds to Location in JS-25 API, but extended to long. Care must be taken to handle
    /// rollover for JS-25 API, this value is expected to never roll over.
    /// </summary>
    public long Encoder { get; init; }

    /// <summary>
    /// monotonically increasing number, corresponds to SequenceNumber in JS-25 SimpleProfile, care
    /// must be taken to handle rollover there. For Pinchot, simple pass-through
    /// </summary>
    public uint SequenceNumber { get; init; }

    /// <summary>
    /// The time stamp when that profile was taken. For JS-25, the time stamps are unsynchronized between heads, and
    /// will roll over. For Pinchot, the time stamps are all in system time, in ns
    /// </summary>
    public ulong TimeStampNs { get; init; }

    /// <summary>
    /// Corresponds to CableID for JS-25 and ScanHeadID for Pinchot.
    /// </summary>
    public uint ScanHeadId { get; init; }

    /// <summary>
    /// For JS-25 this is always 1, for JS-50 Pinchot, it is the index of the camera enum there,
    /// with 0 being invalid.
    /// </summary>
    public uint CameraIndex { get; init; }

    /// <summary>
    /// Placeholder class, as Pinchot does not have input flags (yet)
    /// </summary>
    public InputFlags Inputs { get; init; }

    public IReadOnlyCollection<Point2D> Data => data;
    private Point2D[] data { get; init; }

    public Rect BoundingBox { get; private set; } = Rect.Empty;

    public Profile(Profile original, Point2D[]? newPoints = null)
    {
        Units = original.Units;
        ScanningFlags = original.ScanningFlags;
        LaserIndex = original.LaserIndex;
        LaserOnTimeUs = original.LaserOnTimeUs;
        Encoder = original.Encoder;
        SequenceNumber = original.SequenceNumber;
        TimeStampNs = original.TimeStampNs;
        ScanHeadId = original.ScanHeadId;
        CameraIndex = original.CameraIndex;
        Inputs = original.Inputs;
        if (newPoints != null)
        {
            data = new Point2D[newPoints.Length];
            Array.Copy(newPoints, data, newPoints.Length);
            BoundingBox = Geometry.BoundingBox.Get(data);
        }
        else
        {
            data = new Point2D[original.data.Length];
            Array.Copy(original.data, data, original.data.Length);
            BoundingBox = original.BoundingBox;
        }
        
    }

    public static Profile ConvertTo(Profile original, UnitSystem to)
    {
        if (original.Units == to)
        {
            return original;
        }

        var p = Build(to,
            original.ScanHeadId,
            original.LaserIndex,
            original.CameraIndex,
            original.LaserOnTimeUs,
            original.Encoder,
            original.SequenceNumber,
            original.TimeStampNs,
            original.ScanningFlags,
            original.Inputs,
            (original.Units == UnitSystem.Inches
                ? original.Data.Select(q => new Point2D(q.X * 25.4, q.Y * 25.4, q.B))
                : original.Data.Select(q => new Point2D(q.X / 25.4, q.Y / 25.4, q.B))).ToArray());
        return p;
        
       
    }

    public static Profile BuildWith(Profile profile, Point2D[] toArray)
    {
        throw new NotImplementedException();
    }
}
