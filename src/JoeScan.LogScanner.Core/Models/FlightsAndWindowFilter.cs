using JoeScan.LogScanner.Core.Filters;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using Newtonsoft.Json;

namespace JoeScan.LogScanner.Core.Models;

public class FlightsAndWindowFilter : IFlightsAndWindowFilter
{
    //TODO: for now, we only allow one filter per head, so a dictionary is fine here
    public Dictionary<uint, IFilterShape> Filters = new Dictionary<uint, IFilterShape>();

    public FlightsAndWindowFilter()
    {

        var filters = JsonConvert.DeserializeObject<List<FilterBase>>(File.ReadAllText("rawfilters.json"));

    }

    public Profile Apply(Profile p)
    {
        if (Filters.ContainsKey(p.ScanHeadId))
        {
            p.Data = p.Data.Where(q => Filters[p.ScanHeadId].Contains(q)).ToArray();
            BoundingBox.UpdateBoundingBox(p);
        }
        return p;
    }

    public IEnumerable<uint> FilteredHeads => Filters.Keys;
    public IFilterShape this[uint key] => Filters[key];
}
