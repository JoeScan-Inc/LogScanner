using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Desktop.Engine;

namespace JoeScan.LogScanner.Desktop.StatusBar;

public class StatusBarViewModel : Screen
{
    public EngineViewModel Model { get; }
    public EncoderStatusViewModel EncStatus { get; }
    public string BuildInfo => $"TODO-FIXME ";
    public IObservableCollection<IScannerAdapter> Adapters => Model.Adapters;
    public IScannerAdapter? SelectedAdapter
    {
        get => Model.ActiveAdapter;
        set => Model!.ActiveAdapter = value;
    }

    public StatusBarViewModel(
        EngineViewModel model,
        EncoderStatusViewModel encStatus)
    {
        Model = model;
        EncStatus = encStatus;
    }

   
}
