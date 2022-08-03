using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.Shared.Log3D;

namespace JoeScan.LogScanner.LogReview.Log3D;

public class Log3DWrapperViewModel : Screen
{
    public Log3DViewModel WrappedViewModel { get; }
    public ILogModelObservable Model { get; }

    public Log3DWrapperViewModel(Log3DViewModel wrappedViewModel, ILogModelObservable model)
    {
        WrappedViewModel = wrappedViewModel;
        Model = model;
        Model.PropertyChanged += (_, _) => DisplayLog();
    }

    private void DisplayLog()
    {
        WrappedViewModel.CurrentLogModel = Model.CurrentLogModel;

    }
}
