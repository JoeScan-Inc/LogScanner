using Caliburn.Micro;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.StatusBar;

public class EncoderStatusViewModel : Screen
{
    public string EncoderValue { get; private set; }

    public EncoderStatusViewModel(LogScannerEngine engine)
    {
        engine.EncoderUpdated += Engine_EncoderUpdated;
        EncoderValue = "n/a";
    }

    private void Engine_EncoderUpdated(object? sender, EncoderUpdateArgs e)
    {
        EncoderValue = e.EncoderValue.ToString();
        NotifyOfPropertyChange(()=>EncoderValue);
    }
}
