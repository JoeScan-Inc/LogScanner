using JoeScan.LogScanner.Core.Geometry;

namespace JoeScan.LogScanner.Core.Filters;

public class PolygonFilter
{
    public Point2D[] Vertices { get; set; }
    public IReadOnlyList<Point2D> Outline => Vertices;

    public bool Contains(Point2D p)
    {
        if (Vertices == null)
        {
            return true;
        }

        bool result = false;
        int j = Vertices.Length - 1;
        for (int i = 0; i < Vertices.Length; i++)
        {
            if ((Vertices[i].Y < p.Y && Vertices[j].Y >= p.Y) || (Vertices[j].Y < p.Y && Vertices[i].Y >= p.Y))
            {
                if (Vertices[i].X +
                    ((p.Y - Vertices[i].Y) / (Vertices[j].Y - Vertices[i].Y) * (Vertices[j].X - Vertices[i].X)) < p.X)
                {
                    result = !result;
                }
            }

            j = i;
        }

        return result;
    }
}
