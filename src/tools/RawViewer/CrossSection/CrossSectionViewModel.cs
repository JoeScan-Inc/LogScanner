using Caliburn.Micro;
using JoeScan.LogScanner.Shared.Helpers;
using NLog;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using RawViewer.Helpers;
using RawViewer.Models;
using System.ComponentModel;
using System.Linq;

namespace RawViewer.CrossSection;

public class CrossSectionViewModel : Screen
{
    private LinearAxis columnAxis;
    private LinearAxis rowAxis;
    public DataManager DataManager { get; }
    public PlotColorService PlotColorService { get; }
    public ILogger Logger { get; }

    public PlotModel CrossSectionPlot { get; set; }

    public CrossSectionViewModel(DataManager dataManager, PlotColorService plotColorService, ILogger logger)
    {
        DataManager = dataManager;
        PlotColorService = plotColorService;
        Logger = logger;
        SetupPlot();
        DataManager.PropertyChanged += DataManagerOnPropertyChanged;
    }

    private void DataManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        
        if (e.PropertyName != nameof(DataManager.SelectedProfile))
        {
            return;
        }
        var p = DataManager.SelectedProfile;
        CrossSectionPlot.Series.Clear();
        CrossSectionPlot.Annotations.Clear();
        if (p != null)
        {
            var series = new ScatterSeries()
            {
                MarkerFill = ColorDefinitions.OxyColorForCableId(p.ScanHeadId),
                MarkerStroke = ColorDefinitions.OxyColorForCableId(p.ScanHeadId),
                MarkerType = MarkerType.Plus,
                MarkerSize = 1
            };
            CrossSectionPlot.Series.Add(series);
            series.Points.AddRange(p.Data.Select(q=>new ScatterPoint(q.X,q.Y)));
            CrossSectionPlot.Annotations.Add(new RectangleAnnotation()
            {
                MinimumX = p.Profile.BoundingBox.Left,
                MaximumX = p.Profile.BoundingBox.Right,
                MinimumY = p.Profile.BoundingBox.Bottom,
                MaximumY = p.Profile.BoundingBox.Top,
                Stroke = OxyColor.FromArgb(100,ColorDefinitions.OxyColorForCableId(p.ScanHeadId).R,
                    ColorDefinitions.OxyColorForCableId(p.ScanHeadId).G, ColorDefinitions.OxyColorForCableId(p.ScanHeadId).B),
                StrokeThickness = 1,
                Fill = OxyColors.Transparent
                
            });
        }
        CrossSectionPlot.InvalidatePlot(true);
    }

    private void SetupPlot()
    {
        CrossSectionPlot = new PlotModel
        {
            PlotType = PlotType.Cartesian,
            Background = PlotColorService.PlotBackgroundColor,
            PlotAreaBorderColor = PlotColorService.PlotAreaBorderColor, // not visible anyway
            PlotAreaBorderThickness = new OxyThickness(1),
            // PlotMargins = new OxyThickness(0)
        };

        CrossSectionPlot.Legends.Add(new Legend
        {
            LegendPosition = LegendPosition.BottomRight,
            LegendTextColor = PlotColorService.LegendTextColor,
            LegendBackground = OxyColors.Transparent,
            IsLegendVisible = true
        });

        columnAxis = new LinearAxis
        {
            Minimum = -500,
            Maximum = 500,
            PositionAtZeroCrossing = true,
            AxislineStyle = LineStyle.Solid,
            AxislineColor = PlotColorService.MajorGridLineColor,
            AxislineThickness = 1.3,
            TickStyle = TickStyle.Inside,
            TicklineColor = PlotColorService.MinorGridLineColor,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = PlotColorService.MajorGridLineColor,
            MinorGridlineColor = PlotColorService.MinorGridLineColor,
            IsZoomEnabled = true,
            TextColor = PlotColorService.AxisTextColor,
            Position = AxisPosition.Bottom,
             LabelFormatter = q => $"{q} mm"
        };
        CrossSectionPlot.Axes.Add(columnAxis);

        rowAxis = new LinearAxis
        {
            Minimum = -200,
            Maximum = 800,
            PositionAtZeroCrossing = true,
            AxislineStyle = LineStyle.Solid,
            AxislineColor = PlotColorService.MajorGridLineColor,
            AxislineThickness = 1.3,
            TickStyle = TickStyle.Inside,
            TicklineColor = PlotColorService.MinorGridLineColor,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = PlotColorService.MajorGridLineColor,
            MinorGridlineColor = PlotColorService.MinorGridLineColor,
            IsZoomEnabled = true,
            TextColor = PlotColorService.AxisTextColor,
            Position = AxisPosition.Left,
            LabelFormatter = q => $"{q} mm"
        };
        CrossSectionPlot.Axes.Add(rowAxis);
    }
}
