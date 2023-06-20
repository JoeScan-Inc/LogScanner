using Caliburn.Micro;
using JoeScan.LogScanner.Shared.Helpers;
using MvvmDialogs;
using NLog;
using OxyPlot;
using System.Collections.ObjectModel;
using OxyPlot.Axes;
using OxyPlot.Legends;
using RawViewer.CrossSection;
using RawViewer.Grid;
using RawViewer.ProfileDetail;
using RawViewer.Timeline;
using RawViewer.Toolbar;
using System.Threading;
using System.Threading.Tasks;
using AxisPosition = OxyPlot.Axes.AxisPosition;
using LegendPosition = OxyPlot.Legends.LegendPosition;
using AdonisUI.Controls;
using System.Windows;
using RawViewer.Models;

namespace RawViewer.Shell;

public class ShellViewModel : Screen, IHandle<bool>
{
    public ToolbarViewModel ToolBar { get; }
    public DataManager Data { get; }
    public RawProfileGridViewModel DataGridView { get; }
    public TimelinePlotViewModel TimelinePlot { get; }
    public CrossSectionViewModel CrossSection { get; }
    public ProfileDetailViewModel ProfileDetail { get; }
    public IEventAggregator EventAggregator { get; }
    public ILogger Logger { get; }
    private readonly IDialogService dialogService;
    private readonly IRawViewerConfig config;
    private RawProfile? selectedProfile;
    private bool isBusy;


    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (value == isBusy) return;
            isBusy = value;
            NotifyOfPropertyChange(() => IsBusy);
        }
    }


    public PlotModel? LiveView { get; private set; }

    public ShellViewModel(ToolbarViewModel toolBar, DataManager dataManager,
        RawProfileGridViewModel dataGridView,
        TimelinePlotViewModel timelinePlot ,
        CrossSectionViewModel crossSection, 
        ProfileDetailViewModel profileDetail,
        IEventAggregator eventAggregator,
        IDialogService dialogService, 
        IRawViewerConfig config,
        ILogger logger)
    {
        ToolBar = toolBar;
        Data= dataManager;
        DataGridView = dataGridView;
        TimelinePlot = timelinePlot;
        CrossSection = crossSection;
        ProfileDetail = profileDetail;
        EventAggregator = eventAggregator;
        EventAggregator.Subscribe(this);
        Logger = logger;
        this.dialogService = dialogService;
        this.config = config;
        
    }

    
    public Task HandleAsync(bool message, CancellationToken cancellationToken)
    {
        return Task.Run(()=>IsBusy = message, cancellationToken);
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await CheckBeforeClosing(cancellationToken);
    }

    private Task<bool> CheckBeforeClosing(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.Run((() =>
        {
            
            // save the state of the application, needs to be done on UI thread, so Invoke()
            Application.Current.Dispatcher.Invoke(() =>
            {
               
                // config.EncoderPulseInterval = Data.EncoderPulseInterval;
            });
            return true;
        }), cancellationToken);
    }
}
