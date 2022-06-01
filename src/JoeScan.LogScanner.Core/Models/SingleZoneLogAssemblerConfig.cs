using Autofac.Features.AttributeFilters;
using Nini.Config;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

public class SingleZoneLogAssemblerConfig
{
    public SingleZoneLogAssemblerConfig([KeyFilter("Core.ini")] IConfigSource configSource,
        ILogger logger)
    {

        StartLogCount = configSource.Configs["SingleZoneLogAssembler"].GetInt("StartLogCount");
        StopLogCount = configSource.Configs["SingleZoneLogAssembler"].GetInt("StopLogCount");
        MinLogLength = configSource.Configs["SingleZoneLogAssembler"].GetDouble("MinimumLogLength");
        MaxLogLength = configSource.Configs["SingleZoneLogAssembler"].GetDouble("MaximumLogLength");
        MinProfileSpacing = configSource.Configs["SingleZoneLogAssembler"].GetDouble("MinProfileSpacing");
        StartScanInverted = configSource.Configs["SingleZoneLogAssembler"].GetBoolean("StartScanInputInverted", false);
        UseLogPresenceSignal = configSource.Configs["SingleZoneLogAssembler"].GetBoolean("UseLogPresenceSignal");
        EncoderPulseInterval = configSource.Configs["SingleZoneLogAssembler"].GetDouble("EncoderPulseInterval");

        logger.Debug("StartLogCount = {0}", StartLogCount);
        logger.Debug("StopLogCount = {0}", StopLogCount);
        logger.Debug("MinimumLogLength = {0}", MinLogLength);
        logger.Debug("MaximumLogLength = {0}", MaxLogLength);
        logger.Debug("MinProfileSpacing = {0}", MinProfileSpacing);
        logger.Debug("StartScanInverted = {0}", StartScanInverted);
        logger.Debug("UseLogPresenceSignal = {0}", UseLogPresenceSignal);
        logger.Debug("EncoderPulseInterval = {0}", EncoderPulseInterval);


    }

    public bool UseLogPresenceSignal { get; set; }

    public bool StartScanInverted { get; set; }

    public double MinProfileSpacing { get; set; }

    public double MaxLogLength { get; set; }

    public double MinLogLength { get; set; }

    public int StopLogCount { get; set; }

    public int StartLogCount { get; set; }
    public double EncoderPulseInterval { get; set; }
}
