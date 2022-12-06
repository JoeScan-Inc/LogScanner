using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Desktop.Engine;

namespace JoeScan.LogScanner.Desktop.StatusBar;

public class StatusBarViewModel : Screen
{
    public EngineViewModel Model { get; }
    public EncoderStatusViewModel EncStatus { get; }
    public string BuildInfo => $"TODO-FIXME ";
    public  string NextSolutionNumber { get; set; }
    public IObservableCollection<IScannerAdapter> Adapters => Model.Adapters;
    public IScannerAdapter? SelectedAdapter
    {
        get => Model.ActiveAdapter;
        set => Model!.ActiveAdapter = value;
    }

    public StatusBarViewModel(
        EngineViewModel model,
        EncoderStatusViewModel encStatus,
        IPieceNumberProvider pieceNumberProvider)
    {
        Model = model;
        EncStatus = encStatus;
        pieceNumberProvider.PieceNumberChanged += (_, _) =>
        {
            NextSolutionNumber = pieceNumberProvider.PeekNextPieceNumber().ToString();
            Refresh();
        };
        NextSolutionNumber = pieceNumberProvider.PeekNextPieceNumber().ToString();
    }

   
}
