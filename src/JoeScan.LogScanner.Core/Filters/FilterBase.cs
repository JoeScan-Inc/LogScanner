using JoeScan.LogScanner.Core.Geometry;
using JsonSubTypes;
using Newtonsoft.Json;

namespace JoeScan.LogScanner.Core.Filters;

// we use the excellent JsonSubTypes package to make it possible 
// to deserialize polymorphic types with Json.NET. 
// https://github.com/manuc66/JsonSubTypes

[JsonConverter(typeof(JsonSubtypes), "Kind")]
[JsonSubtypes.KnownSubType(typeof(PolygonFilter), "PolygonFilter")]
[JsonSubtypes.KnownSubType(typeof(BoxFilter), "BoxFilter")]
public abstract class FilterBase 
{
    public abstract string Kind { get; }
    public uint ScanHeadId { get; set; }
    public abstract bool Contains(Point2D p);
    public abstract IReadOnlyList<Point2D> Outline { get; }
    public bool IsEnabled { get; set; }
    public abstract bool IsValid { get;  }


}
