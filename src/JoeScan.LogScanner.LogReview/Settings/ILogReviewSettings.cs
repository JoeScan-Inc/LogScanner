using Config.Net;
using JoeScan.LogScanner.Shared.Enums;

namespace JoeScan.LogScanner.LogReview.Settings;

public interface ILogReviewSettings
{
    string FileBrowserLastFolder { get; set; }
    [Option(DefaultValue = "Inches")]
    DisplayUnits Units { get; set; }
}
