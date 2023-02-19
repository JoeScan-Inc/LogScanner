using Config.Net;
using JoeScan.LogScanner.Shared.Enums;

namespace JoeScan.LogScanner.LogReview.Config;

public interface ILogReviewConfig
{
    string FileBrowserLastFolder { get; set; }
    [Option(DefaultValue = "Inches")]
    DisplayUnits Units { get; set; }

    [Option(DefaultValue = true)]
    bool ShowAcceptedPoints { get; set; }
    [Option(DefaultValue = true)]

    bool ShowRejectedPoints { get; set; }
    [Option(DefaultValue = true)]
    bool ShowModelPoints { get; set; }
    [Option(DefaultValue = true)]
    bool ShowModel { get; set; }
    [Option(DefaultValue = true)]
    bool ShowSectionCenters { get; set; }
}
