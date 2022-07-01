using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Geometry;
using System.Diagnostics;

namespace JoeScan.LogScanner.Core.Models;
[DebuggerDisplay("Center = {SectionCenter}, Accepted = {AcceptedPoints.Count}, Rejected = {RejectedPoints.Count}")]

public class LogSection
{
    public bool IsValid { get; set; }

    #region Immutable Properties

    public IReadOnlyList<Profile> Profiles { get; init; }
    public double SectionCenter { get; init; }
    public List<Point2D> AcceptedPoints { get; init; }
    public List<Point2D> ModeledProfile { get; init; }
    public List<Point2D> RejectedPoints { get; init; }
    public FilteredBoundingBox BoundingBox { get; init; }
    public Ellipse EllipseModel { get; init; }
    public double BarkAllowance { get; init; }

    #endregion

    #region Lazily Evaluated Backing

    private Lazy<double> rawDiameterX;

    private Lazy<double> totalArea;

    public Lazy<double> rawDiameterY;

    private Lazy<double> woodArea;

    private Lazy<double> fitError;
    #endregion

    #region Public Properties

    public double RawDiameterMax => EllipseModel.A * 2;
    public double RawDiameterMin => EllipseModel.B * 2;
    public double DiameterMaxAngle => EllipseModel.Theta;
    public double CentroidX => EllipseModel.X;
    public double CentroidY => EllipseModel.Y;

    public double RawDiameterX => rawDiameterX.Value;
    public double RawDiameterY => rawDiameterY.Value;

    public double TotalArea => totalArea.Value;

    public double DiameterMax => RawDiameterMax - 2.0 * BarkAllowance;
    public double DiameterMin => RawDiameterMin - 2.0 * BarkAllowance;

    public double Ovality => DiameterMax / DiameterMin;
    
    public double WoodArea => woodArea.Value;

    public double BarkArea => TotalArea - WoodArea;
    public double FitError => fitError.Value;

    #endregion

    #region Lifecycle

    internal LogSection(IReadOnlyList<Profile> profiles, double sectionCenter)
    {
        IsValid = false;
        Profiles = profiles;
        SectionCenter = sectionCenter;
        rawDiameterX = new Lazy<double>(() => Math.Sqrt(Math.Pow(RawDiameterMax * Math.Cos(DiameterMaxAngle), 2.0) + Math.Pow(RawDiameterMin * Math.Sin(DiameterMaxAngle), 2.0)), true);
        rawDiameterY = new Lazy<double>(() => Math.Sqrt(Math.Pow(RawDiameterMax * Math.Sin(DiameterMaxAngle), 2.0) + Math.Pow(RawDiameterMin * Math.Cos(DiameterMaxAngle), 2.0)), true);
        totalArea =  new Lazy<double>(() => Math.PI * RawDiameterMax / 2 * RawDiameterMin / 2, true);
        woodArea = new Lazy<double>(() => Math.PI * (RawDiameterMax / 2.0 - BarkAllowance) * (RawDiameterMin / 2.0 - BarkAllowance), true);
        fitError = new Lazy<double>(() => (EllipseFit.LeastSquaresEllipseError(RawDiameterMax / 2.0, RawDiameterMin / 2.0, DiameterMaxAngle,
            CentroidX, CentroidY, AcceptedPoints!.ToFloatArray())), true);
    }

    #endregion

    public double GetFitError(List<Point2D> pts)
    {
        return EllipseFit.LeastSquaresEllipseError(RawDiameterMax / 2.0, RawDiameterMin / 2.0, DiameterMaxAngle,
            CentroidX, CentroidY, pts.ToFloatArray());
    }
}
