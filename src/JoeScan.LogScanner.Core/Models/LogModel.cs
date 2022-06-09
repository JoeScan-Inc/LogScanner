namespace JoeScan.LogScanner.Core.Models;

public class LogModel
{
    /// <summary>
    /// Data and measurements at each section
    /// </summary>
    public List<LogSection> Sections = new List<LogSection>();

    /// <summary>
    /// Data and measurements at each section
    /// </summary>
    public readonly List<LogSection> RejectedSections = new List<LogSection>();

    /// <summary>
    /// Log Number
    /// </summary>
    public int LogNumber { get; internal set; }

    /// <summary>
    /// The interval between sections in millimeters.
    /// </summary>
    public double Interval { get; private set; }

    /// <summary>
    /// Log length in millimeters.
    /// </summary>
    public double Length { get; private set; }

    /// <summary>
    /// Date and Time when this log was scanned
    /// </summary>
    public DateTime TimeScanned { get; internal set; }
}
