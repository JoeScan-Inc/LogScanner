using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.Shared.Live3D;

namespace JoeScan.LogScanner.LogReview.Log3D;

public class Log3DWrapperViewModel : Screen
{
    public Live3DViewModel WrappedViewModel { get; }
    public ILogModelObservable Model { get; }

    //TODO: review, this seems unnecessary, why not use model directly?
    public Log3DWrapperViewModel(Live3DViewModel wrappedViewModel, ILogModelObservable model)
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
