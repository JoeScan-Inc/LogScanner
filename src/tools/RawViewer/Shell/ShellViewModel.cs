using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Shared.Helpers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Nini.Config;
using NLog;
using OxyPlot;
using SkiaSharp;
using System;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using JoeScan.LogScanner.Shared.Enums;
using LiveChartsCore.Measure;
using NLog.Filters;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using RawViewer.Grid;
using RawViewer.Timeline;
using RawViewer.Toolbar;
using System.Diagnostics;
using System.Drawing;
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
    public IEventAggregator EventAggregator { get; }
    public ILogger Logger { get; }
    private readonly IDialogService dialogService;
    private readonly IRawViewerConfig config;
    private RawProfile? selectedProfile;
    private bool isBusy;

    public ObservableCollection<ISeries> Series { get; set; } = new ObservableCollection<ISeries>();

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
        TimelinePlotViewModel timelinePlot ,IEventAggregator eventAggregator,
        IDialogService dialogService, IRawViewerConfig config, ILogger logger)
    {
        ToolBar = toolBar;
        Data= dataManager;
        DataGridView = dataGridView;
        TimelinePlot = timelinePlot;
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
