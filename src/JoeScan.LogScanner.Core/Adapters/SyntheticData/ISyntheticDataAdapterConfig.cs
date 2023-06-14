using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core.Adapters.SyntheticData;

public interface ISyntheticDataAdapterConfig
{
    public UnitSystem Units { get; }
    public double EncoderPulseInterval { get; } // in Unit e.g. mm or inch
    public double ChainSpeed { get; } // in Units/s e.g.  mm/s or inch/s
    public double MinLogLength { get; }
    public double MaxLogLength { get; }
    public double MinLogDiameter { get; }
    public double MaxLogDiameter { get; }
    public double MaxDiameterVariation { get; }
    public double MaxCurvature { get; }

    public double LeadingGap { get; }
    public double TrailingGap { get; }
}
