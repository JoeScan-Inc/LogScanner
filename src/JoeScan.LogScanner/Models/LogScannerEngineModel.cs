
using Caliburn.Micro;
using JoeScan.LogScanner.Config;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Models;

public class LogScannerEngineModel : PropertyChangedBase
{
    private LogScannerEngine Engine { get; }
    public ILogger Logger { get; }

    public LogScannerEngineModel(LogScannerEngine engine,
        ILogger logger,
        ILogScannerConfig config)
    {
        Engine = engine;
        Logger = logger;
        Adapters = new BindableCollection<IScannerAdapter>(Engine.AvailableAdapters);
        if (!string.IsNullOrEmpty(config.ActiveAdapter))
        {
            var n = Adapters.FirstOrDefault(q => q.Name == config.ActiveAdapter);
            {
                if (n != null)
                {
                    try
                    {
                        ActiveAdapter = n;
                    }
                    catch (Exception e)
                    {
                        var msg = $"Error setting active adapter: {e.Message}";
                        Logger.Error(msg);
                        Engine.SetActiveAdapter(Engine.AvailableAdapters.First());
                        Logger.Warn($"Fallback adapter: {Engine.ActiveAdapter!.Name}");
                    }
                }
            }
        }
       
        Engine.AdapterChanged += (_, _) => Refresh();
        Engine.ScanningStarted += (_, _) => Refresh();
        Engine.ScanningStopped += (_, _) => Refresh();
        Engine.ScanErrorEncountered += (_, _) => Refresh();

        // Engine.EncoderUpdated += ... // don't use
        // engine.LogModelBroadcastBlock.LinkTo(new ActionBlock<LogModel>(model =>
        // {
        //     // needs to run on UI thread
        //     Application.Current.Dispatcher.BeginInvoke(() => log3D.CurrentLogModel = model);
        // }));
    }
    public IObservableCollection<IScannerAdapter> Adapters { get; }

    public IScannerAdapter? ActiveAdapter
    {
        get => Engine.ActiveAdapter;
        set
        {
            Engine.SetActiveAdapter(value);
        }
    }

    public bool CanStart =>  ActiveAdapter != null && !Engine.IsRunning;
    public bool IsRunning => Engine.IsRunning;
    public bool CanStop => IsRunning;


    #region Observables

    public BroadcastBlock<Profile> RawProfilesBroadcast => Engine.RawProfilesBroadcastBlock;
    public BroadcastBlock<RawLog> RawLogBroadcast => Engine.RawLogsBroadcastBlock;
    public BroadcastBlock<LogModel> LogModelBroadcast => Engine.LogModelBroadcastBlock;

    #endregion


    public async void Start()
    {
         await Task.Run(()=>Engine.Start());
         NotifyOfPropertyChange(()=>IsRunning);
    }

    public async void Stop()
    {
        await Task.Run(() => Engine.Stop());
        NotifyOfPropertyChange(() => IsRunning);

    }

    public void StartDumping()
    {
       Engine.StartDumping();
    }

    public void StopDumping()
    {
        Engine.StartDumping();
    }

}
