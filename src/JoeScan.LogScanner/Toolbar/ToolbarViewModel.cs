using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using System;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace JoeScan.LogScanner.Toolbar;

public class ToolbarViewModel : Screen
{
    private bool record;
    private string selectedAdapter;
    private LogScannerEngine Engine { get; }

    public IObservableCollection<IScannerAdapter> Adapters =>
        new BindableCollection<IScannerAdapter>(Engine.AvailableAdapters);

    public IScannerAdapter? SelectedAdapter
    {
        get => Engine!.ActiveAdapter;
        set => Engine!.SetActiveAdapter(value);
    }

    public ToolbarViewModel(LogScannerEngine engine)
    {
        Engine = engine;
        Engine.ScanningStopped += EngineStateChanged!;
        Engine.ScanningStarted += EngineStateChanged!;
        //TODO: save and restore in settings
        SelectedAdapter = Engine.AvailableAdapters.First();
    }

    private void EngineStateChanged(object sender, EventArgs args)
    {
        NotifyOfPropertyChange(()=>CanStart);
        NotifyOfPropertyChange(()=>CanStop);
    }

    public bool CanStart => Engine.CanStart;
    public bool CanStop => Engine.IsRunning;

    public void Start()
    {
        Engine.Start(); 
    }
    public void Stop()
    {
        Engine.Stop();
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
                Engine.StartDumping();
            }
            else
            {
                Engine.StopDumping();
            }
            record = value;

        }
    }
}
