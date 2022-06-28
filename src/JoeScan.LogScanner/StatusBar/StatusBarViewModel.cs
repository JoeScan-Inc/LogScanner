using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.StatusBar;

public class StatusBarViewModel : Screen
{
    public LogScannerEngine Engine { get; }
    public EncoderStatusViewModel EncStatus { get; }
    public string BuildInfo => $"v{GitVersionInformation.FullSemVer} ";
    public string Adapter  => Engine.ActiveAdapter != null ? Engine.ActiveAdapter.Name : "n/a";
    public string EngineUnits => Engine.Units.ToString();

    public StatusBarViewModel(LogScannerEngine engine,
        EncoderStatusViewModel encStatus)
    {
        Engine = engine;
        EncStatus = encStatus;
        Engine.AdapterChanged += (_, _) =>
        {
            Refresh();
        };
    }

   
}
