using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Interfaces;

namespace JoeScan.LogScanner.Core.Models;

class PieceNumberProvider : IPieceNumberProvider
{
    public ICoreConfig Config { get; }
    private static volatile int nextLogNumber = 1;

    public int GetNextPieceNumber()
    {
        return nextLogNumber++;
    }

    public PieceNumberProvider(ICoreConfig config)
    {
        Config = config;
    }
}
