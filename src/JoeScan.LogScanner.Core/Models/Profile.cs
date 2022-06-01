using JoeScan.LogScanner.Core.Geometry;

namespace JoeScan.LogScanner.Core.Models;
/// <summary>
/// This is a generic profile, representing a single scan. It is assumed to be in
/// mill coordinate system already.
/// </summary>
public class Profile
{
    /// <summary>
    /// Private Constructor
    /// </summary>
    public Profile()
    {
        Data = Array.Empty<Point2D>();
        ScanningFlags = ScanFlags.None;
        EncoderValues = new Dictionary<uint, long>();
        Inputs = InputFlags.None;
        // rest is 0-initialized
    }

    public Point2D[] Data { get; set; }
    /// <summary>
    /// encapsulates the JS-25 flags value, for Pinchot this is empty as the profile there does not
    /// have any
    /// </summary>
    public ScanFlags ScanningFlags { get; set; }
    /// <summary>
    /// LaserIndex is always 1 for JS-25 and corresponds to the enum index for JS-50 Pinchot, in
    /// both cases 0 is invalid. Also note that this is uint and not int as in JS-25 interface
    /// </summary>
    public uint LaserIndex { get; set; }
    /// <summary>
    /// LaserOn time in microseconds. 
    /// </summary>
    public ushort LaserOnTimeUs { get; set; }
    /// <summary>
    /// corresponds to Location in JS-25 API, but extended to long. Care must be taken to handle
    /// rollover for JS-25 API, this value is expected to never roll over.
    /// </summary>
    public IDictionary<uint, long> EncoderValues { get; set; }
    /// <summary>
    /// monotonically increasing number, corresponds to SequenceNumber in JS-25 SimpleProfile, care
    /// must be taken to handle rollover there. For Pinchot, simple pass-through
    /// </summary>
    public uint SequenceNumber { get; set; }
    /// <summary>
    /// The time stamp when that profile was taken. For JS-25, the time stamps are unsynchronized between heads, and
    /// will roll over. For Pinchot, the time stamps are all in system time, in ns
    /// </summary>
    public ulong TimeStampNs { get; set; }
    /// <summary>
    /// Corresponds to CableID for JS-25 and ScanHeadID for Pinchot.
    /// </summary>
    public uint ScanHeadId { get; set; }
    /// <summary>
    /// For JS-25 this is always 1, for JS-50 Pinchot, it is the index of the camera enum there,
    /// with 0 being invalid.
    /// </summary>
    public uint Camera { get; set; }
    /// <summary>
    /// Placeholder class, as Pinchot does not have input flags (yet)
    /// </summary>
    public InputFlags Inputs { get; set; }

    internal Rect BoundingBox { get; set; } = Rect.Empty;

    // TODO: add container to hold filtered data instead of throwing it away

}
