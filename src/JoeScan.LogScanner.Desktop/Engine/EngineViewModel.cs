using Caliburn.Micro;
using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Desktop.Config;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Desktop.Engine;

public class EngineViewModel : PropertyChangedBase
{
    public LogScannerEngine Engine { get; }
    public ILogger Logger { get; }
    public IObservableCollection<IScannerAdapter> Adapters { get; }

    public IScannerAdapter? ActiveAdapter
    {
        get => Engine.ActiveAdapter;
        set
        {
            Engine.SetActiveAdapter(value);
        }
    }

    public bool CanStart => ActiveAdapter != null && !Engine.IsRunning;
    public bool IsRunning => Engine.IsRunning;
    public bool CanStop => IsRunning;
    public BroadcastBlock<Profile> RawProfilesBroadcast => Engine.RawProfilesBroadcastBlock;
    public BroadcastBlock<LogModelResult> LogModelBroadcastBlock => Engine.LogModelBroadcastBlock;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;
    public EngineViewModel(LogScannerEngine engine,
        ILogScannerConfig config,
        ILogger logger)
    {
        Engine = engine;
        Logger = logger;
        engine.EncoderUpdated += (sender, args) => OnEncoderUpdated(args);
        Adapters = new BindableCollection<IScannerAdapter>(Engine.AvailableAdapters);
        // check the config to see what adapter to use
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
        else
        {
            //no adapter set in config, use the first one in list
            if (Adapters.Count > 0)
            {
                Engine.SetActiveAdapter(Engine.AvailableAdapters.First());
            }
            else
            {
                // no adapter found, we can't possibly continue

                var msg = $"No adapters discovered. Can not continue.";
                Logger.Error(msg);
                throw (new Exception(msg));
            }

        }

        Engine.AdapterChanged += (_, _) => Refresh();
        Engine.ScanningStarted += (_, _) => Refresh();
        Engine.ScanningStopped += (_, _) => Refresh();
        Engine.ScanErrorEncountered += (_, _) => Refresh();
    }
    
    public void Start()
    {
        Engine.Start();
        NotifyOfPropertyChange(() => IsRunning);
    }

    public void Stop()
    {
        Engine.Stop();
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

    protected virtual void OnEncoderUpdated(EncoderUpdateArgs e)
    {
        EncoderUpdated?.Invoke(this, e);
    }
}
