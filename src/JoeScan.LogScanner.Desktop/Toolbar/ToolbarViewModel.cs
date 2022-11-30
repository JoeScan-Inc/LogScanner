using Caliburn.Micro;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Desktop.Engine;
using NLog;

// ReSharper disable MemberCanBePrivate.Global

namespace JoeScan.LogScanner.Desktop.Toolbar;

public class ToolbarViewModel : Screen
{
    public ILogger Logger { get; }
    private bool record;
    private string selectedAdapter;
    private EngineViewModel Model { get; }

    

    

    public ToolbarViewModel(EngineViewModel model,
        ILogger logger)
    {
        Model = model;
        Logger = logger;
        Model!.PropertyChanged += (_, _) => Refresh();
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
