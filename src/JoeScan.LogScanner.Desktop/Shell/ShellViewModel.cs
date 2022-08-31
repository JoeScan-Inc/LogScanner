using Caliburn.Micro;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.Main;
using NLog;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace JoeScan.LogScanner.Desktop.Shell;

public class ShellViewModel : Screen
{
    public MainViewModel UI { get; }
    public ILogger Logger { get; }
    public ILogScannerConfig Config { get; }

    public string Title => "JoeScan LogScanner";

    public ShellViewModel(
        ILogger logger,
        ILogScannerConfig config,   
        MainViewModel ui
        )
    {
        Logger = logger;
        Config = config;
        UI = ui;
    }
    //TODO: implement window state persistence

}
