using JoeScan.LogScanner.Core.Filters;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Core.Models;

public class FlightsAndWindowFilter
    : IFlightsAndWindowFilter
{
    public Dictionary<uint, PolygonFilter> Filters = new Dictionary<uint, PolygonFilter>();

    public FlightsAndWindowFilter()
    {


    }

    public Profile Apply(Profile p)
    {
        if (Filters.ContainsKey(p.ScanHeadId))
        {
            // modify the data in place,
            // see also issue #4
            p.Data = p.Data.Where(q => Filters[p.ScanHeadId].Contains(q)).ToArray();
            BoundingBox.UpdateBoundingBox(p);
        }
        return p;
    }

    public IEnumerable<uint> FilteredHeads => Filters.Keys;
    public PolygonFilter this[uint key] => Filters[key];
}
