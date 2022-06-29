using JoeScan.LogScanner.Core.Geometry;
using System.Diagnostics;

namespace JoeScan.LogScanner.Core.Models;
[DebuggerDisplay("Center = {SectionCenter}, Accepted = {FilteredPoints.Count}, Rejected = {RejectedPoints.Count}")]

public class LogSection
{
    public IReadOnlyList<Profile> Profiles { get; init; }
    public double SectionCenter { get; init; }
    public bool IsValid { get; set; }

    public List<Point2D> FilteredPoints { get; init; }

    public List<Point2D> ModeledProfile { get; init; }

    public List<Point2D> RejectedPoints { get; init; }

    public FilteredBoundingBox BoundingBox { get; init; }

    public Ellipse EllipseModel { get; init; }

    public double RawDiameterMax => EllipseModel.A * 2;
    public double RawDiameterMin => EllipseModel.B * 2;
    public double DiameterMaxAngle => EllipseModel.Theta;
    public double CentroidX => EllipseModel.X;
    public double CentroidY => EllipseModel.Y;

    private Lazy<double> rawDiameterX =>
        new(() => Math.Sqrt(Math.Pow(RawDiameterMax * Math.Cos(DiameterMaxAngle), 2.0) + Math.Pow(RawDiameterMin * Math.Sin(DiameterMaxAngle), 2.0)), true);
    public double RawDiameterX => rawDiameterX.Value;
    public Lazy<double> rawDiameterY =>
        new(() => Math.Sqrt(Math.Pow(RawDiameterMax * Math.Sin(DiameterMaxAngle), 2.0) + Math.Pow(RawDiameterMin * Math.Cos(DiameterMaxAngle), 2.0)), true);
    public double RawDiameterY => rawDiameterY.Value;

    private Lazy<double> totalArea => new(() => Math.PI * RawDiameterMax / 2 * RawDiameterMin / 2, true);
    public double TotalArea => totalArea.Value;

    public double DiameterMax => RawDiameterMax - 2.0 * BarkAllowance;
    public double DiameterMin => RawDiameterMin - 2.0 * BarkAllowance;

    public double Ovality => DiameterMax / DiameterMin;

    public double BarkAllowance
    {
        get;
        set;
    }
    private Lazy<double> woodArea =>
        new Lazy<double>(() => Math.PI * (RawDiameterMax / 2.0 - BarkAllowance) * (RawDiameterMin / 2.0 - BarkAllowance), true);
    public double WoodArea => woodArea.Value;

    public double BarkArea => TotalArea - WoodArea;

    internal LogSection(IReadOnlyList<Profile> profiles, double sectionCenter)
    {
        IsValid = false;
        Profiles = profiles;
        SectionCenter = sectionCenter;

    }
}
