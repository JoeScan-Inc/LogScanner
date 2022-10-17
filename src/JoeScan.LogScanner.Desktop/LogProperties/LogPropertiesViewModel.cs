using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.Engine;
using NLog;
using System.Threading.Tasks.Dataflow;
using System.Windows;

namespace JoeScan.LogScanner.Desktop.LogProperties;

public class LogPropertiesViewModel : Screen
{
    private LogDisplayModel currentLogModel;
    public ILogScannerConfig Config { get; }
    public ILogger Logger { get; }
    public EngineViewModel Model { get; }

    public IObservableCollection<LogPropertyItemViewModel> Items { get; set; }
        = new BindableCollection<LogPropertyItemViewModel>();
    public LogDisplayModel? CurrentLogModel
    {
        get => currentLogModel;
        set
        {
            if (Equals(value, currentLogModel))
            {
                return;
            }
            currentLogModel = value;
            NotifyOfPropertyChange(() => CurrentLogModel);
            RefreshDisplay();
        }
    }

    private void RefreshDisplay()
    {
        foreach (var item in Items)
        {
            item.UpdateWith(new LogData() { Length = 42.0 });
        }
    }

    public LogPropertiesViewModel(ILogScannerConfig config,
        ILogger logger,
        EngineViewModel model)
    {
        Config = config;
        Logger = logger;
        Model = model;

        Model.LogModelBroadcastBlock.LinkTo(new ActionBlock<LogModel>((logModel =>
        {
            Application.Current.Dispatcher.BeginInvoke(() => CurrentLogModel = new LogDisplayModel(logModel));
        })));

        // get all properties on LogData that are decorated with the [Unit] attribute
        foreach (var propertyInfo in typeof(LogModel).GetProperties())
        {
            object[] attribute = propertyInfo.GetCustomAttributes(typeof(UnitAttribute), true);
            if (attribute.Length > 0)
            {
               // LogModelProperties.Add(propertyInfo.Name);
            }
        }




    }
}
