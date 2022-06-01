using JoeScan.LogScanner.Core.Geometry;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IFilterShape
{
    uint ScanHeadId { get; }
    bool Contains(Point2D p);

    IReadOnlyList<Point2D> Outline { get; }

}
