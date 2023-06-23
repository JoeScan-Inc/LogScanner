using JoeScan.LogScanner.Core.Interfaces;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

internal class RawProfileValidator : IRawProfileValidator
{
    public ILogger Logger { get; }

    public RawProfileValidator(
        ILogger logger)
    {
        Logger = logger;
    }

    public bool IsValid(Profile p)
    {
        //TODO: use better heuristics
        //TODO: this may iterate over valid points twice
        return p.NumValidPoints > 20;
    }
}
