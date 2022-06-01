namespace JoeScan.LogScanner.Core.Geometry;

public struct Rect
{
    public static readonly Rect Empty;

    public double X;
    public double Y;
    public double Width;
    public double Height;
    public bool IsEmpty => Equals(default(Rect));

    public Rect(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }


    public double Right => X + Width;
    public double Left => X;
    public double Top => Y + Height;
    public double Bottom => Y;

    public bool Contains(Rect other)
    {
        if (IsEmpty || other.IsEmpty)
        {
            return false;
        }
        return Left <= other.Left
               && Top >= other.Top
               && Right >= other.Right
                && Bottom <= other.Bottom;
    }
    public bool IntersectsWith(Rect other)
    {
        if (IsEmpty || other.IsEmpty)
        {
            return false;
        }
        return Left < other.Right
               && Right > other.Left
               && Top > other.Bottom
               && Bottom < other.Top;

    }

    public static bool IntersectsWith(Rect a, Rect b)
    {
        return a.IntersectsWith(b);
    }
}
