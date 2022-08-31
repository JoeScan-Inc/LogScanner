using Caliburn.Micro;
using JoeScan.LogScanner.Desktop.Engine;
using JoeScan.LogScanner.Desktop.Main;

namespace JoeScan.LogScanner.Desktop.StatusBar;

public class StatusBarViewModel : Screen
{
    public EngineViewModel Model { get; }
    public EncoderStatusViewModel EncStatus { get; }
    public string BuildInfo => $"v{GitVersionInformation.FullSemVer} ";
   

    public StatusBarViewModel(
        EngineViewModel model,
        EncoderStatusViewModel encStatus)
    {
        Model = model;
        EncStatus = encStatus;
    }

   
}
