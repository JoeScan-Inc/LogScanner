using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Interfaces;
using Nini.Config;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

class RawProfileValidator : IRawProfileValidator
{
    public IConfigSource Config { get; }

    public RawProfileValidator([KeyFilter("Core.ini")] IConfigSource config,
        ILogger logger)
    {
        Config = config;
    }

    public bool IsValid(Profile p)
    {
        //TODO: use better heuristics
        return p.Data.Length > 20;
    }
}
