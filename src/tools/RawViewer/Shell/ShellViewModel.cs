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
        IDialogService dialogService, IRawViewerConfig config, ILogger logger)
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
        SetupPlotModel();
    }

    

    private void SetupPlotModel()
    {
        LiveView = new PlotModel
        {
            PlotType = PlotType.Cartesian,
            Background = OxyColorsForStyle.PlotBackgroundColor,
            PlotAreaBorderColor = OxyColorsForStyle.PlotAreaBorderColor,
            PlotAreaBorderThickness = new OxyThickness(0),
            PlotMargins = new OxyThickness(-10)
        };

        LiveView.Legends.Add(new Legend
        {
            LegendPosition = LegendPosition.TopRight,
            LegendTextColor = OxyColorsForStyle.LegendTextColor
        });

        
        var columnAxis = new LinearAxis
        {
            Minimum = -100 ,
            Maximum = 500,
            PositionAtZeroCrossing = true,
            AxislineStyle = LineStyle.Solid,
            AxislineColor = OxyColorsForStyle.MajorGridLineColor,
            AxislineThickness = 1.3,
            TickStyle = TickStyle.Crossing,
            TicklineColor = OxyColorsForStyle.MinorGridLineColor,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColorsForStyle.MajorGridLineColor,
            MinorGridlineColor = OxyColorsForStyle.MinorGridLineColor,
            IsZoomEnabled = true,
            TextColor = OxyColorsForStyle.AxisTextColor,
          
        };
        LiveView.Axes.Add(columnAxis);

        var rowAxis = new LinearAxis
        {
            Minimum = -300,
            Maximum = 300,
            Position = AxisPosition.Bottom,
            PositionAtZeroCrossing = true,
            AxislineStyle = LineStyle.Solid,
            AxislineColor = OxyColorsForStyle.MajorGridLineColor,
            AxislineThickness = 1.3,
            TickStyle = TickStyle.Crossing,
            TicklineColor = OxyColorsForStyle.MinorGridLineColor,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColorsForStyle.MajorGridLineColor,
            MinorGridlineColor = OxyColorsForStyle.MinorGridLineColor,
            IsZoomEnabled = true,
            TextColor = OxyColorsForStyle.AxisTextColor,
        };
        LiveView.Axes.Add(rowAxis);

       

    }


    public Task HandleAsync(bool message, CancellationToken cancellationToken)
    {
        return Task.Run(()=>IsBusy = message, cancellationToken);
    }
}
