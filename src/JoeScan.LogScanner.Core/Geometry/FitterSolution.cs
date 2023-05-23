namespace JoeScan.LogScanner.Core.Geometry;
public struct FitterSolution
{
    public double Radius { get; init; }
    public double XOffset { get; init; }
    public double XSlope { get; init; }
    public double YOffset { get; init; }
    public double YSlope { get; init; }

    public FitterSolution()
    {
       
        XOffset = 0;
        XSlope = 0;
        YOffset = 0;
        YSlope = 0;
        Radius = 0;
    }
}
