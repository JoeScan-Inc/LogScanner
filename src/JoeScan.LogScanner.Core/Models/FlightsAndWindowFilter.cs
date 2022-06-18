using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Filters;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Interfaces;
using Newtonsoft.Json;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

public class FlightsAndWindowFilter : IFlightsAndWindowFilter
{
    public IConfigLocator ConfigLocator { get; }

    //TODO: for now, we only allow one filter per head, so a dictionary is fine here
    public Dictionary<uint, FilterBase> Filters = new Dictionary<uint, FilterBase>();

    public FlightsAndWindowFilter(IConfigLocator configLocator, ILogger logger)
    {
        ConfigLocator = configLocator;
        try
        {
            var filters = JsonConvert.DeserializeObject<List<FilterBase>>(File.ReadAllText(Path.Combine(ConfigLocator.GetConfigLocation(),"rawfilters.json")));
         
            if (filters != null)
            {
                Filters = filters.Where(q=>q.IsValid && q.IsEnabled).ToDictionary(q => q.ScanHeadId);
                foreach (uint f in Filters.Keys)
                {
                    logger.Debug($"{Filters[f].Kind} for scan head {f} added.");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Warn($"failed to read one or more raw point filters: {ex.Message}");
        }
    }

    public Profile Apply(Profile p)
    {
        if (Filters.ContainsKey(p.ScanHeadId))
        {
            if (Filters[p.ScanHeadId].IsEnabled)
            {
                p.Data = p.Data.Where(q => Filters[p.ScanHeadId].Contains(q)).ToArray();
                BoundingBox.UpdateBoundingBox(p);
            }
        }
        return p;
    }

    public IEnumerable<uint> FilteredHeads => Filters.Keys;
    public FilterBase this[uint key] => Filters[key];
}
