using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Geometry;
using System.Runtime.ExceptionServices;
using UnitsNet;
using UnitsNet.Units;
#pragma warning disable CS0618

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

    #region Lazy Backing Values

    private Lazy<double> length;
    private Lazy<double> beginZ;
    private Lazy<double> endZ;
    private Lazy<Profile> lastGoodProfile;
    private Lazy<Profile> firstGoodProfile;
    private Lazy<Point3D> centerLineStart;
    private Lazy<Point3D> centerLineEnd;
    private Lazy<double> taper;
    private Lazy<double> taperX;
    private Lazy<double> taperY;
   
    #endregion

    #region Lifecycle

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
        centerLineStart = new Lazy<Point3D>(() => new Point3D(
            Sections.First().SectionCenter * CenterLineSlopeX + CenterLineInterceptXZ,
            Sections.First().SectionCenter * CenterLineSlopeY + CenterLineInterceptYZ,
            Sections.First().SectionCenter,
            0.0), true);
        centerLineEnd = new Lazy<Point3D>(() => new Point3D(
            Sections.Last().SectionCenter * CenterLineSlopeX + CenterLineInterceptXZ,
            Sections.Last().SectionCenter * CenterLineSlopeY + CenterLineInterceptYZ,
            Sections.Last().SectionCenter,
            0.0), true);
        taper = new Lazy<double>(() => (LargeEndDiameter - SmallEndDiameter) / Length,true);
        taperX = new Lazy<double>(() => (LargeEndDiameterX - SmallEndDiameterX) / Length,true);
        taperY = new Lazy<double>(() => (LargeEndDiameterY - SmallEndDiameterY) / Length,true);
    }

    #endregion

    /// <summary>
    /// Log length 
    /// </summary>
  
    public double Length => length.Value;
    public Profile LastGoodProfile => lastGoodProfile.Value;
    public Profile FirstGoodProfile => firstGoodProfile.Value;

    public double CenterLineSlopeX { get; internal set; }
  
    public double CenterLineInterceptXZ { get; internal set; }

    public double CenterLineSlopeY { get; internal set; }
  
    public double CenterLineInterceptYZ { get; internal set; }
    public Point3D CenterLineStart => centerLineStart.Value;
    public Point3D CenterLineEnd => centerLineEnd.Value;
   
    public double SmallEndDiameter { get; internal set; }
   
    public double SmallEndDiameterX { get; internal set; }
   
    public double SmallEndDiameterY { get; internal set; }
  
    public double LargeEndDiameter { get; internal set; }
  
    public double LargeEndDiameterX { get; internal set; }
  
    public double LargeEndDiameterY { get; internal set; }
    public double Sweep { get; internal set; }
    
    public double SweepAngleRad { get; internal set; }
  
    public double CompoundSweep { get; internal set; }
  
    public double CompoundSweep90 { get; internal set; }
    public double Taper => taper.Value;
    public double TaperX => taperX.Value;
    public double TaperY => taperY.Value;
    public double Volume { get; internal set; }
    public double BarkVolume { get; internal set; }

    public double MaxDiameter { get; internal set; }
    public double MaxDiameterZ { get; internal set; }
    public double MinDiameter { get; internal set; }
    public double MinDiameterZ { get; internal set; }
    public bool ButtEndFirst { get; internal set; }

    public RawLog RawLog { get; set; }
}
