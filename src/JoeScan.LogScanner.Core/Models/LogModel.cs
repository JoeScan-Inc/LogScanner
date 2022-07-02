using JoeScan.LogScanner.Core.Extensions;
using System.Runtime.ExceptionServices;

namespace JoeScan.LogScanner.Core.Models;

public class LogModel
{
    private readonly double maxFitError;
    private readonly double encoderPulseInterval;

    #region Immutable Properties

    /// <summary>
    /// Log Number
    /// </summary>
    public int LogNumber { get;  }

    /// <summary>
    /// The interval between sections in millimeters.
    /// </summary>
    public double Interval { get;  }

    /// <summary>
    /// Date and Time when this log was scanned
    /// </summary>
    public DateTime TimeScanned { get;  }

    /// <summary>
    /// Data and measurements at each section
    /// </summary>
    public List<LogSection> Sections { get; init; }= new List<LogSection>();

    /// <summary>
    /// Data and measurements at each section
    /// </summary>
    public  List<LogSection> RejectedSections { get; init; } = new List<LogSection>();

    public double EncoderPulseInterval => encoderPulseInterval;

    #endregion

    private Lazy<double> length;
    private Lazy<double> beginZ;
    private Lazy<double> endZ;
    private Lazy<Profile> lastGoodProfile;
    private Lazy<Profile> firstGoodProfile;

    internal LogModel(int logNumber, double interval, DateTime timeScanned, double maxFitError, double encoderPulseInterval)
    {
        this.maxFitError = maxFitError;
        this.encoderPulseInterval = encoderPulseInterval;
        LogNumber = logNumber;
        Interval = interval;
        TimeScanned = timeScanned;
        length = new Lazy<double>(() => (LastGoodProfile.EncoderValues[0] - FirstGoodProfile.EncoderValues[0])*encoderPulseInterval);
        lastGoodProfile = new Lazy<Profile>(() =>
        {
            var section = Sections.Last();
            for (int i = section.Profiles.Count - 1; i >= 0; i--)
            {
                if (section.GetFitError(section.Profiles[i].Data.ToList()) < maxFitError)
                {
                    return section.Profiles[i];
                }
            }
            // ugh...
            return section.Profiles.Last();
        },true);
        firstGoodProfile = new Lazy<Profile>(() =>
        {
            var section = Sections.First();
            foreach (var t in section.Profiles)
            {
                if (section.GetFitError(t.Data.ToList()) < maxFitError)
                {
                    return t;
                }
            }
            return section.Profiles.First();
        }, true);
    }

    /// <summary>
    /// Log length in millimeters.
    /// </summary>
    public double Length => length.Value;
    public Profile LastGoodProfile => lastGoodProfile.Value;
    public Profile FirstGoodProfile => firstGoodProfile.Value;
}
