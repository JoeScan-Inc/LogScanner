using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Core.Filters;

public class BoxFilter : IFilterShape
{
    private Rect rect;

    public BoxFilter(Rect r)
    {
        rect = r;
    }

    public bool IntersectsWith(Rect r)
    {
        return rect.IntersectsWith(r);
    }

    public bool Contains(Rect r)
    {
        return rect.Contains(r);
    }

    public IReadOnlyList<Point2D> Outline => new List<Point2D>()
    {
        new Point2D(rect.Left, rect.Bottom, 0),
        new Point2D(rect.Left, rect.Top, 0),
        new Point2D(rect.Right, rect.Top, 0),
        new Point2D(rect.Right, rect.Bottom, 0)
    };

    public uint ScanHeadId { get; }

    public bool Contains(Point2D p)
    {
        return p.X > rect.Left && p.X < rect.Right && p.Y > rect.Bottom && p.Y < rect.Top;
    }
}
