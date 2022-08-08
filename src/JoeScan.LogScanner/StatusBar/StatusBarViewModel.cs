using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Models;

namespace JoeScan.LogScanner.StatusBar;

public class StatusBarViewModel : Screen
{
    public LogScannerEngineModel Model { get; }
    public EncoderStatusViewModel EncStatus { get; }
    public string BuildInfo => $"v{GitVersionInformation.FullSemVer} ";
    public string Adapter  => Model.ActiveAdapter != null ? Model.ActiveAdapter.Name : "n/a";
   

    public StatusBarViewModel(LogScannerEngineModel model,
        EncoderStatusViewModel encStatus)
    {
        Model = model;
        EncStatus = encStatus;
        
    }

   
}
