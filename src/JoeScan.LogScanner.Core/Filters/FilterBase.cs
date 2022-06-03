using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using JsonSubTypes;
using Newtonsoft.Json;

namespace JoeScan.LogScanner.Core.Filters;

[JsonConverter(typeof(JsonSubtypes), "Kind")]
[JsonSubtypes.KnownSubType(typeof(PolygonFilter), "PolygonFilter")]
[JsonSubtypes.KnownSubType(typeof(BoxFilter), "BoxFilter")]
public abstract class FilterBase : IFilterShape
{
    public abstract string Kind { get; }
    public uint ScanHeadId { get; set; }
    public abstract bool Contains(Point2D p);
    public abstract IReadOnlyList<Point2D> Outline { get; }

}
