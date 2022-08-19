using Caliburn.Micro;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Desktop.Engine;
using NLog;
// ReSharper disable ClassNeverInstantiated.Global

namespace JoeScan.LogScanner.Desktop.StatusBar;

public class EncoderStatusViewModel : Screen
{
    public EngineViewModel Model { get; }
    public ILogger Logger { get; }
    public string EncoderValue { get; private set; }

    public EncoderStatusViewModel(EngineViewModel model,
        ILogger logger)
    {
        Model = model;
        Logger = logger;
        Model.EncoderUpdated += Engine_EncoderUpdated;
        EncoderValue = "n/a";
    }

    private void Engine_EncoderUpdated(object? sender, EncoderUpdateArgs e)
    {
        EncoderValue = e.EncoderValue.ToString();
        NotifyOfPropertyChange(()=>EncoderValue);
    }
}
