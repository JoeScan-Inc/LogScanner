using Accessibility;
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Helpers;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace JoeScan.LogScanner.LiveProfiles;

public sealed class LiveProfileViewModel : Screen
{
    #region Private Fields

    private readonly Dictionary<Tuple<uint, uint>, ScatterSeries> idToSeries = new();
    private readonly DispatcherTimer dispatcherTimer;
    private readonly int refreshIntervalMs = 50;


    #endregion

    #region Pipeline Endpoint

    private readonly ActionBlock<Profile> displayActionBlock;
    private bool paused;
    private readonly ConcurrentDictionary<Tuple<uint, uint>, Profile> headCamDict
        = new ConcurrentDictionary<Tuple<uint, uint>, Profile>();

    #endregion

    #region Lifecycle

    public LiveProfileViewModel(LogScannerEngine engine)
    {
        paused = false;
        Engine = engine;
        SetupPlotModel();
        dispatcherTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, refreshIntervalMs)
        };
        dispatcherTimer.Tick += (_, _) => DrawPreview();
        Engine.ScanningStarted += (_, _) => dispatcherTimer.Start();
        Engine.ScanningStopped += (_, _) => dispatcherTimer.Stop();


        displayActionBlock = new ActionBlock<Profile>(StoreProfiles);
        engine.RawProfiles.LinkTo(displayActionBlock);

    }

    #endregion

    #region Injected Properties

    public LogScannerEngine Engine { get; }

    #endregion

    #region Bound Properties

    public PlotModel? LiveView { get; private set; }

    public bool Paused
    {
        get => paused;
        set
        {
            if (value == paused) return;
            paused = value;
            NotifyOfPropertyChange(() => Paused);
        }
    }

    #endregion

    #region Private Methods



    private void StoreProfiles(Profile profile)
    {
        // we just save the latest profile, the UI refresh happens 
        // on a fixed timer basis. This is neccessary because at 
        // high scan rates we would get bogged down quickly 

        // we used scan head id and camera as the index
        var t = new Tuple<uint, uint>(profile.ScanHeadId, profile.Camera);
        headCamDict[t] = profile;
        
    }

    private void DrawPreview()
    {
        // we want to draw profiles from both cameras on a single WX head
        // on separate series, but with similar colors
        foreach (var key in headCamDict.Keys)
        {
             var series = GetSeries(key);
             series.Points.Clear();
            series.Points.AddRange(headCamDict[key].Data.Select(q => new ScatterPoint(q.X, q.Y)));
             LiveView!.InvalidatePlot(true);

        }
        headCamDict.Clear();
        // var series = GetSeries(profile.ScanHeadId);
        // if (paused)
        // {
        //     series.Title = $"ID: {profile.ScanHeadId} Enc: {profile.EncoderValues[0]}";
        //     LiveView.InvalidatePlot(false);
        // }
        // else
        // {
        //     series.Title = $"ID: {profile.ScanHeadId} Enc: {profile.EncoderValues[0]}";
        //     series.Points.Clear();
        //     series.Points.AddRange(profile.Data.Select(q => new ScatterPoint(q.X, q.Y)));
        //     LiveView.InvalidatePlot(true);
        // }
    }

    private ScatterSeries GetSeries(Tuple<uint, uint> idCameraPair)
    {
        if (!idToSeries.ContainsKey(idCameraPair))
        {
            idToSeries[idCameraPair] = new ScatterSeries
            {
                Title = $"Head: {idCameraPair.Item1} Camera {idCameraPair.Item2}",
                MarkerStroke = ColorDefinitions.OxyColorForCableId(idCameraPair.Item1,(int)idCameraPair.Item2),
                MarkerType = MarkerType.Plus,
                MarkerSize = 1.0,
                MarkerFill = OxyColors.Transparent
            };
            LiveView.Series.Add(idToSeries[idCameraPair]);
            var orderedSeries = LiveView.Series.OrderBy(q => q.Title).ToArray();
            LiveView.Series.Clear();
            foreach (var series in orderedSeries)
            {
                LiveView.Series.Add(series);
            }
        }

        return idToSeries[idCameraPair];
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
            Minimum = -100,
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
            // LabelFormatter = x => null
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
            // LabelFormatter = x => null
        };
        LiveView.Axes.Add(rowAxis);
        var filterOutline = new PolygonAnnotation()
        {
            Layer = AnnotationLayer.BelowSeries,
            Fill = OxyColors.Transparent,
            Stroke = OxyColors.LightGray,
            StrokeThickness = 1.0,
            LineStyle = LineStyle.Dash
        };
      //  filterOutline.Points.AddRange(Filter.Outline.Select(q => new DataPoint(q.X, q.Y)));
        LiveView.Annotations.Add(filterOutline);
    }

    #endregion
}
