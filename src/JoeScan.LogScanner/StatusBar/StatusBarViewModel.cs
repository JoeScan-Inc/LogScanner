using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.StatusBar;

public class StatusBarViewModel : Screen
{
    private readonly LogScannerEngine engine;
    public string BuildInfo => $"v{GitVersionInformation.FullSemVer} ";
    public string Adapter => engine.ScannerAdapter.Name;
    public string EngineUnits => engine.Units.ToString();

    public StatusBarViewModel(LogScannerEngine engine)
    {
        this.engine = engine;
    }

   
}
