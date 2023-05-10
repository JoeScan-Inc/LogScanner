using Caliburn.Micro;
using NLog;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot;
using OxyPlot.Series;
using RawViewer.Helpers;
using RawViewer.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using JoeScan.LogScanner.Shared.Helpers;
using OxyPlot.Annotations;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace RawViewer.Timeline;

public class TimelinePlotViewModel : Screen
{
    private PlotModel timeLinePlot;
    private LinearAxis columnAxis;
    private LinearAxis rowAxis;
    private Func<RawProfile, Tuple<double, double>> selectedPlotFunction;
    private LineAnnotation positionMarker;
    public DataManager DataManager { get; }
    public PlotColorService PlotColorService { get; }
    public ILogger Logger { get; }


    public IObservableCollection<KeyValuePair<string, Func<RawProfile, Tuple<double, double>>>> PlotFunctions { get; }
        = new BindableCollection<KeyValuePair<string, Func<RawProfile, Tuple<double, double>>>>();

    public Func<RawProfile, Tuple<double, double>> SelectedPlotFunction
    {
        get => selectedPlotFunction;
        set
        {
            if (Equals(value, selectedPlotFunction))
            {
                return;
            }
            selectedPlotFunction = value;
            NotifyOfPropertyChange(() => SelectedPlotFunction);
            RefreshPlot();
        }
    }

    public PlotModel TimeLinePlot => timeLinePlot;

    public TimelinePlotViewModel(DataManager dataManager, PlotColorService plotColorService, ILogger logger)
    {
        DataManager = dataManager;
        PlotColorService = plotColorService;
        Logger = logger;
        SetupPlot();
        DataManager.ProfileDataAdded += (_, _) => RefreshPlot();
        DataManager.HeadSelectionChanged += (_, _) => RefreshPlot();
        DataManager.PropertyChanged += SelectedProfileChanged;

        PlotFunctions.Add(new KeyValuePair<string, Func<RawProfile, Tuple<double, double>>>("Pts over Encoder",
            new Func<RawProfile, Tuple<double, double>>((p) => new Tuple<double, double>((double)p.ReducedEncoder, p.NumPts))));
        PlotFunctions.Add(new KeyValuePair<string,Func<RawProfile,Tuple<double,double>>>("Encoder over Time",
            new Func<RawProfile, Tuple<double, double>>((p) => new Tuple<double, double>((double)p.ReducedTimeStampNs/1000, (double)p.ReducedEncoder))));
        PlotFunctions.Add(new KeyValuePair<string, Func<RawProfile, Tuple<double, double>>>("Pts over Time",
            new Func<RawProfile, Tuple<double, double>>((p) => new Tuple<double, double>((double)p.ReducedTimeStampNs / 1000, p.NumPts))));
        PlotFunctions.Add(new KeyValuePair<string, Func<RawProfile, Tuple<double, double>>>("LaserOnTime over Time",
            new Func<RawProfile, Tuple<double, double>>((p) => new Tuple<double, double>((double)p.ReducedTimeStampNs / 1000, p.LaserOnTimeUs))));
        PlotFunctions.Add(new KeyValuePair<string, Func<RawProfile, Tuple<double, double>>>("Avg Brightness Over Time",
            new Func<RawProfile, Tuple<double, double>>((p) => new Tuple<double, double>((double)p.ReducedTimeStampNs / 1000,
                p.Data.Length > 0 ? p.Data.Average(q=>q.B) : 0))));
        PlotFunctions.Add(new KeyValuePair<string, Func<RawProfile, Tuple<double, double>>>("Pts over Z",
            new Func<RawProfile, Tuple<double, double>>((p) => new Tuple<double, double>((double)p.ReducedEncoder * DataManager.EncoderPulseInterval, p.NumPts))));
        SelectedPlotFunction = PlotFunctions[0].Value;
    }

    private void SelectedProfileChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DataManager.SelectedProfile))
        {
            return;
        }

        if (DataManager.SelectedProfile != null)
        {
            var pos = selectedPlotFunction(DataManager.SelectedProfile);
            if (!timeLinePlot.Annotations.Contains(positionMarker))
            {
                timeLinePlot.Annotations.Add(positionMarker);
            }
            positionMarker.X = pos.Item1;
            positionMarker.Color = ColorDefinitions.OxyColorForCableId(DataManager.SelectedProfile.ScanHeadId);
        }
        else
        {
            if (timeLinePlot.Annotations.Count > 0)
            {
                timeLinePlot.Annotations.Clear();
            }
        }
        timeLinePlot.InvalidatePlot(false);
    }

    private void RefreshPlot()
    {
        timeLinePlot.Series.Clear();
        var headIds = DataManager.ScanHeadFilterById < 0
            ? DataManager.SelectableHeads.Skip(1).Select(q => q.Key)
            : new[] { DataManager.ScanHeadFilterById };
        var xAxisMin = Double.MaxValue;
        var xAxisMax = Double.MinValue;
        var yAxisMin = Double.MaxValue;
        var yAxisMax = Double.MinValue;

        foreach (var id in headIds)
        {
            var series = new ScatterSeries()
            {
                Title = $"Head {id}",
                MarkerStroke = ColorDefinitions.OxyColorForCableId((uint)id),
                MarkerType = MarkerType.Square,
                MarkerFill = ColorDefinitions.OxyColorForCableId((uint)id),
                MarkerSize = 1
            };
            series.Points.AddRange(DataManager.Profiles.Where(q => q.ScanHeadId == id)
                .Select(q =>
                {
                    var t = SelectedPlotFunction(q);
                    xAxisMin = Math.Min(xAxisMin, t.Item1);
                    xAxisMax = Math.Max(xAxisMax, t.Item1);
                    yAxisMin = Math.Min(yAxisMin, t.Item2);
                    yAxisMax = Math.Max(yAxisMax, t.Item2);
                    return new ScatterPoint(t.Item1, t.Item2,double.NaN,double.NaN,q);
                }));

            series.MouseDown += SeriesOnMouseDown;
            timeLinePlot.Series.Add(series);
            timeLinePlot.ResetAllAxes();
            columnAxis.Minimum = xAxisMin;
            columnAxis.Maximum = xAxisMax;
            rowAxis.Minimum = yAxisMin;
            rowAxis.Maximum = yAxisMax;
        }


        timeLinePlot.InvalidatePlot(true);
    }

    private void SeriesOnMouseDown(object? sender, OxyMouseDownEventArgs e)
    {
       // Logger.Trace($"Series: {e.HitTestResult.Element}, Index: {e.HitTestResult.Index}");
       if (e.HitTestResult.Element is ScatterSeries series)
       {
           if (e.HitTestResult.Item is ScatterPoint pt)
           {
               var profile = pt.Tag as RawProfile;
               if (profile != null)
               {
                   DataManager.SelectedProfile = profile;
               }
           }
       }
    }

    private void SetupPlot()
    {
        timeLinePlot = new PlotModel
        {
            PlotType = PlotType.XY,
            Background = PlotColorService.PlotBackgroundColor,
            PlotAreaBorderColor = PlotColorService.PlotAreaBorderColor, // not visible anyway
            PlotAreaBorderThickness = new OxyThickness(1),
            // PlotMargins = new OxyThickness(0)
        };

        timeLinePlot.Legends.Add(new Legend
        {
            LegendPosition = LegendPosition.BottomRight,
            LegendTextColor = PlotColorService.LegendTextColor,
            LegendBackground = OxyColors.Transparent,
            IsLegendVisible = true
        });

        columnAxis = new LinearAxis
        {
            Minimum = -30,
            Maximum = 30,
            PositionAtZeroCrossing = false,
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
            Position = AxisPosition.Bottom
            // LabelFormatter = q => $"{q} {scanSystemManager.ScanSystemUnits.UiString()}"
        };
        timeLinePlot.Axes.Add(columnAxis);

        rowAxis = new LinearAxis
        {
            Minimum = -30,
            Maximum = 30,
            PositionAtZeroCrossing = false,
            AxislineStyle = LineStyle.Solid,
            AxislineColor = PlotColorService.MajorGridLineColor,
            AxislineThickness = 1.3,
            TickStyle = TickStyle.Inside,
            TicklineColor = PlotColorService.MinorGridLineColor,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = PlotColorService.MajorGridLineColor,
            MinorGridlineColor = PlotColorService.MinorGridLineColor,
            IsZoomEnabled = false,
            TextColor = PlotColorService.AxisTextColor,
            Position = AxisPosition.Left
            // LabelFormatter = q => $"{q} {scanSystemManager.ScanSystemUnits.UiString()}"
        };
        timeLinePlot.Axes.Add(rowAxis);
        positionMarker = new LineAnnotation()
        {
            Type = LineAnnotationType.Vertical,
            X = 0,
            Color = OxyColors.Yellow,
        };
        
    }
}
