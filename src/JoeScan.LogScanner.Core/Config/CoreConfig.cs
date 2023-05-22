using Nini.Config;
using Nini.Ini;
using NLog;
using System.Configuration;
using UnitsNet;
using UnitsNet.Units;

namespace JoeScan.LogScanner.Core.Config;

public class ConfigBase
{
    protected readonly ILogger logger;
    protected LengthUnit baseLengthUnit = LengthUnit.Millimeter;
    protected List<string> Warnings = new List<string>();
    protected List<string> Errors = new List<string>();
    public bool IsValid { get; protected set; } = false;

    protected ConfigBase()
    {
        logger = LogManager.GetCurrentClassLogger();
    }

    protected double ParseAndConvert(string s)
    {
        try
        {
            return Length.Parse(s).ToUnit(baseLengthUnit).Value;
        }
        catch (Exception e)
        {
            Warnings.Add(e.Message);
            logger.Warn(e);
            return double.Parse(s);
        }
    }
}

public class CoreConfig : ConfigBase
{
    public double EncoderPulseInterval { get; init; }

    public CoreConfig(IConfig? config)
    {
        try
        {
            if (config != null)
            {
                EncoderPulseInterval = ParseAndConvert(config.GetString("EncoderPulseInterval"));
                IsValid = true;
            }
            else
            {
                throw new ArgumentNullException("config section [Core] missing");
            }
        }
        catch (Exception e)
        {
            logger.Error(e);
            Errors.Add(e.Message);
        }
    }
}
public class SingleZoneLogAssemblerConfig : ConfigBase
{
    public SingleZoneLogAssemblerConfig(IConfig? config)
    {
        try
        {
            if (config != null)
            {
                UseLogPresenceSignal = config.GetBoolean("UseLogPresenceSignal", false);
                StartScanInverted = config.GetBoolean("StartScanInverted", false);
                MinProfileSpacing = ParseAndConvert(config.GetString("MinProfileSpacing"));
                MaxLogLength = ParseAndConvert(config.GetString("MaxLogLength"));
                MinLogLength = ParseAndConvert(config.GetString("MinLogLength"));
                StartLogCount = config.GetInt("StartLogCount", 10);
                StopLogCount = config.GetInt("StopLogCount", 10);
            }
            else
            {
                throw new ArgumentNullException("config section [SingleZoneLogAssembler] missing");
            }
        }
        catch (Exception e)
        {
            logger.Error(e);
            Errors.Add(e.Message);
        }
    }

    public bool UseLogPresenceSignal { get; init; }
    public bool StartScanInverted { get; init; }
    public double MinProfileSpacing { get; init; }
    public double MaxLogLength { get; init; }
    public double MinLogLength { get; init; }
    public int StopLogCount { get; init; }
    public int StartLogCount { get; init; }

}

public class LogModelBuilderConfig : ConfigBase
{
    public LogModelBuilderConfig(IConfig? config)
    {
        try
        {
            if (config != null)
            {
                SectionInterval = ParseAndConvert(config.GetString("SectionInterval"));
                DiameterEndOffset = ParseAndConvert(config.GetString("DiameterEndOffset"));
                MaxFitError = ParseAndConvert(config.GetString("MaxFitError"));
            }
            else
            {
                throw new ArgumentNullException("config section [LogModelBuilderConfig] missing");
            }
        }
        catch (Exception e)
        {
            logger.Error(e);
            Errors.Add(e.Message);
        }
    }

    public double SectionInterval { get; init; }
    public double DiameterEndOffset { get; init; }
    public double MaxFitError { get; set; }
}

public class RawLogArchiverConfig : ConfigBase
{
    public RawLogArchiverConfig(IConfig config)
    {
        try
        {
            if (config != null)
            {
                Location = config.GetString("Location", "");
                IsEnabled = config.GetBoolean("Enabled", false);
                MaxCount = config.GetInt("MaxCount", 100);
                if (String.IsNullOrEmpty(Location))
                {
                    IsEnabled = false;
                }
            }
            else
            {
                throw new ArgumentNullException("config section [RawLogArchiver] missing");
            }
        }
        catch (Exception e)
        {
            logger.Error(e);
            Errors.Add(e.Message);
        }
    }

    public string Location { get; init; }
    public bool IsEnabled { get; init; }
    public int MaxCount { get; init; }
    
    
}

public class RawDumperConfig : ConfigBase
{
    public RawDumperConfig(IConfig? config)
    {
        try
        {
            if (config != null)
            {
                Location = config.GetString("Location", "");
                HistoryLocation = config.GetString("HistoryLocation", "");
                HistorySize = config.GetInt("HistorySize", 10000);
                HistoryEnabled = config.GetBoolean("HistoryEnabled", false);
            }
            else
            {
                throw new ArgumentNullException("config section [RawDumper] missing");
            }
        }
        catch (Exception e)
        {
            logger.Error(e);
            Errors.Add(e.Message);
        }
    }

    public string Location { get; init; }
    public string HistoryLocation { get; init; }
    public int HistorySize { get; init; }
    public bool HistoryEnabled { get; init; } 
}

public class SectionBuilderConfig : ConfigBase
{
    public SectionBuilderConfig(IConfig? config)
    {
        try
        {
            if (config != null)
            {
                FilterOutliers = config.GetBoolean("FilterOutliers", true);
                OutlierFilterMaxDistance = ParseAndConvert(config.GetString("OutlierFilterMaxDistance"));
                OutlierFilterMaxNumIterations = config.GetInt("OutlierFilterMaxNumIterations", 100);
                BarkAllowance = ParseAndConvert(config.GetString("BarkAllowance","0 mm"));
                ModelPointCount = config.GetInt("ModelPointCount", 100);
                MinimumLogDiameter = ParseAndConvert(config.GetString("MinimumLogDiameter"));
                MaximumLogDiameter = ParseAndConvert(config.GetString("MaximumLogDiameter"));
                LogMaxPositionX = ParseAndConvert(config.GetString("LogMaxPositionX"));
                LogMaxPositionY = ParseAndConvert(config.GetString("LogMaxPositionY"));
                LogMinPositionX = ParseAndConvert(config.GetString("LogMinPositionX"));
                LogMinPositionY = ParseAndConvert(config.GetString("LogMinPositionY"));
                MaxOvality = config.GetDouble("MaxOvality",3);
                 
            }
            else
            {
                throw new ArgumentNullException("config section [SectionBuilder] missing");
            }
        }
        catch (Exception e)
        {
            logger.Error(e);
            Errors.Add(e.Message);
        }
    }

    public bool FilterOutliers { get; init; }
    public double OutlierFilterMaxDistance{ get; init; }
    public int OutlierFilterMaxNumIterations { get; init; }
    public double BarkAllowance { get; init; }
    public int ModelPointCount { get; init; }
    public double MinimumLogDiameter { get; init; }
    public double MaximumLogDiameter { get; init; }
    public double LogMaxPositionX { get; init; }
    public double LogMaxPositionY { get; init; }
    public double LogMinPositionX { get; init; }
    public double LogMinPositionY { get; init; }
    public double MaxOvality { get; set; }
}
