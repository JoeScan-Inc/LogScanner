using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core.Geometry;

public static class BoundingBox
{
    public static Rect Get(Point2D[] pts)
    {
        if (pts.Length < 2)
        {
            return Rect.Empty;
        }
        var minX = pts.Min(q => q.X);
        var minY = pts.Min(q => q.Y);
        var maxX = pts.Max(q => q.X);
        var maxY = pts.Max(q => q.Y);
        return new Rect(minX, minY, (maxX - minX), (maxY - minY));
    }

    public static Profile UpdateBoundingBox(Profile p)
    {
        p.BoundingBox = Get(p.Data);
        return p;
    }
}
