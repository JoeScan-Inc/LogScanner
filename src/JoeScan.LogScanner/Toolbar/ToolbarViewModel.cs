using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Models;
using System;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace JoeScan.LogScanner.Toolbar;

public class ToolbarViewModel : Screen
{
    private bool record;
    private string selectedAdapter;
    private LogScannerEngineModel Model { get; }

    

    public IScannerAdapter? SelectedAdapter => Model.ActiveAdapter;

    public IObservableCollection<IScannerAdapter> Adapters => Model.Adapters;

    public ToolbarViewModel(LogScannerEngineModel model)
    {
        Model = model;
        Model.PropertyChanged += (_, _) => Refresh();

    }

    public bool CanStart => Model.CanStart;
    public bool CanStop => Model.CanStop;

    public void Start()
    {
        Model.Start(); 
    }
    public void Stop()
    {
        Model.Stop();
    }

    public bool Record
    {
        get => record;
        set
        {
            if (value == record)
            {
                return;
            }

            if (value)
            {
                Model.StartDumping();
            }
            else
            {
                Model.StopDumping();
            }
            record = value;

        }
    }
}
