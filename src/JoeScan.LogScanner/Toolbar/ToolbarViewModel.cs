using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using System;
// ReSharper disable MemberCanBePrivate.Global

namespace JoeScan.LogScanner.Toolbar;

public class ToolbarViewModel : Screen
{
    private LogScannerEngine Engine { get; }
    
    public ToolbarViewModel(LogScannerEngine engine)
    {
        Engine = engine;
        Engine.ScanningStopped += EngineStateChanged!;
        Engine.ScanningStarted += EngineStateChanged!;
    }

    private void EngineStateChanged(object sender, EventArgs args)
    {
        NotifyOfPropertyChange(()=>CanStart);
        NotifyOfPropertyChange(()=>CanStop);
    }

    public bool CanStart => !Engine.IsRunning;
    public bool CanStop => !CanStart;

    public void Start()
    {
        Engine.Start(); 
    }
    public void Stop()
    {
        Engine.Stop();
    }
}
