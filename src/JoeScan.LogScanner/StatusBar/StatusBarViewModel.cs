using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.StatusBar;

public class StatusBarViewModel : Screen
{
    public LogScannerEngine Engine { get; }
    public string BuildInfo => $"v{GitVersionInformation.FullSemVer} ";
    public string Adapter  => Engine.ActiveAdapter != null ? Engine.ActiveAdapter.Name : "n/a";
    public string EngineUnits => Engine.Units.ToString();

    public StatusBarViewModel(LogScannerEngine engine)
    {
        Engine = engine;
        Engine.AdapterChanged += (_, _) =>
        {
            Refresh();
        };
    }

   
}
