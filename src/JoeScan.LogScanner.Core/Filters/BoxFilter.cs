using JoeScan.LogScanner.Core.Geometry;

namespace JoeScan.LogScanner.Core.Filters;

public class BoxFilter : FilterBase
{
    
    public double Left { get; set; }
    public double Top { get; set; }
     
    public double Right { get; set; }
    public double Bottom { get; set; }
    
    public override IReadOnlyList<Point2D> Outline => new List<Point2D>()
    {
        new Point2D(Left, Bottom, 0),
        new Point2D(Left, Top, 0),
        new Point2D(Right, Top, 0),
        new Point2D(Right, Bottom, 0)
    };

    public override string Kind => "BoxFilter";

    public override bool Contains(Point2D p)
    {
        return p.X > Left && p.X < Right && p.Y > Bottom && p.Y < Top;
    }

    
}
