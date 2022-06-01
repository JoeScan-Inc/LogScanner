using JoeScan.LogScanner.Core.Geometry;
using System.Drawing;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IFlightsAndWindowFilterConfig
{
    IEnumerable<IPolygonFilter> Filters { get; }
}

public interface IPolygonFilter
{
    uint ScanHeadId { get; set; }
    IEnumerable<PointF> Points { get; }
    bool Contains(Point2D p);
}


