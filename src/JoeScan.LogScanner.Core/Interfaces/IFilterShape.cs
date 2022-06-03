using JoeScan.LogScanner.Core.Geometry;
using JsonSubTypes;
using Newtonsoft.Json;


public interface IFilterShape
{
    // Kind is used to tell the JSON deserializer what implementation
    // to read from the stream: BoxFilter, PolygonFilter, BisectFilter etc
    string Kind { get; }
    uint ScanHeadId { get; }
    bool Contains(Point2D p);

    IReadOnlyList<Point2D> Outline { get; }


}
