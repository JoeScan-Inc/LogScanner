using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using System.Runtime.Serialization;

namespace JoeScan.LogScanner.Core.Filters;

public class PolygonFilter : FilterBase
{
    private bool isValid;
    public override bool IsValid => isValid;
    public List<Point2D>? Vertices { get; set; }
    public override IReadOnlyList<Point2D> Outline => Vertices;

    public override string Kind => "PolygonFilter";

    public override bool Contains(Point2D p)
    {
        if (Vertices == null)
        {
            return true;
        }

        bool result = false;
        int j = Vertices.Count - 1;
        for (int i = 0; i < Vertices.Count; i++)
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
    // this is called after Json.NET created the object, we check here if 
    // it is a valid filter
    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        isValid = Vertices is { Count: > 3 };
    }
}
