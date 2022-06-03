using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Interfaces;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

class RawProfileValidator : IRawProfileValidator
{
    public ICoreConfig Config { get; }

    public RawProfileValidator(ICoreConfig config,
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
