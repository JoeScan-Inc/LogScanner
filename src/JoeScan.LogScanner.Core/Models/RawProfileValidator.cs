using JoeScan.LogScanner.Core.Interfaces;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

internal class RawProfileValidator : IRawProfileValidator
{
    public RawProfileValidator(
        ILogger logger)
    {
    }

    public bool IsValid(Profile p)
    {
        //TODO: use better heuristics
        return p.Data.Count > 20;
    }
}
