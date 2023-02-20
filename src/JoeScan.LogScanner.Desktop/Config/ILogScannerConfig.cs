using Config.Net;
using JoeScan.LogScanner.Shared.Enums;
using OxyPlot.Axes;

namespace JoeScan.LogScanner.Desktop.Config;

public interface ILogScannerConfig
{
    string ActiveAdapter { get; set; }

    [Option(DefaultValue = "Inches")]
    DisplayUnits Units { get; set; }

    ILiveProfileViewConfig LiveProfileConfig { get; set; }
    ILogHistoryViewConfig LogHistoryConfig { get; set; }

    IMessageListConfig MessageListConfig { get; set; }

}

public interface ILiveProfileViewConfig
{
    [Option(DefaultValue = true)]
    bool ShowFilters { get; set; }
}

public interface ILogHistoryViewConfig
{
    [Option(DefaultValue = 100)]
    int MaxLength { get; set; }
}

public interface IMessageListConfig
{
    [Option(DefaultValue = 300)]
    int MaxLength { get; set; }
}
