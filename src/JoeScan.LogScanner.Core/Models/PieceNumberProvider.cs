using Autofac.Features.AttributeFilters;
using JoeScan.LogScanner.Core.Interfaces;
using Nini.Config;

namespace JoeScan.LogScanner.Core.Models;

class PieceNumberProvider : IPieceNumberProvider
{
    public IConfigSource Config { get; }
    private static volatile int nextLogNumber = 1;

    public int GetNextPieceNumber()
    {
        return nextLogNumber++;
    }

    public PieceNumberProvider([KeyFilter("Core.ini")] IConfigSource config)
    {
        Config = config;
    }
}
