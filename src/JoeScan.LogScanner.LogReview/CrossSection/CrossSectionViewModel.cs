using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Extensions;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.Navigator;
using JoeScan.LogScanner.Shared.Helpers;
using NLog;
using NLog.Filters;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Windows.Documents;

// ReSharper disable MemberCanBePrivate.Global

namespace JoeScan.LogScanner.LogReview.CrossSection;

public class CrossSectionViewModel : Screen
{
    #region Backing Properties

    private LogSection? currentSection;
    private Mode viewMode;

    #endregion

    #region Injected Properties

    public ILogger Logger { get; }

    #endregion

    public enum Mode
    {
        ModeSection,
        ModeRawProfiles
    }

    public List<KeyValuePair<Mode, string>> ModeStringPairs => new List<KeyValuePair<Mode, string>>()
    {
        new KeyValuePair<Mode, string>(Mode.ModeSection, "Sections"),
        new KeyValuePair<Mode, string>(Mode.ModeRawProfiles, "Raw Profiles")
    };

    public Mode ViewMode
    {
        get => viewMode;
        set
        {
            if (value == viewMode)
            {
                return;
            }
            viewMode = value;
            NotifyOfPropertyChange(() => ViewMode);
            RefreshDisplay();
        }
    }

    public bool ShowAcceptedPoints
    {
        get => showAcceptedPoints;
        set
        {
            if (value == showAcceptedPoints) return;
            showAcceptedPoints = value;
            NotifyOfPropertyChange(() => ShowAcceptedPoints);
            RefreshDisplay();
        }
    }

    public bool ShowRejectedPoints
    {
        get => showRejectedPoints;
        set
        {
            if (value == showRejectedPoints) return;
            showRejectedPoints = value;
            NotifyOfPropertyChange(() => ShowRejectedPoints);
            RefreshDisplay();
        }
    }

    public bool ShowModelPoints
    {
        get => showModelPoints;
        set
        {
            if (value == showModelPoints) return;
            showModelPoints = value;
            NotifyOfPropertyChange(() => ShowModelPoints);
            RefreshDisplay();
        }
    }

    public bool ShowModel
    {
        get => showModel;
        set
        {
            if (value == showModel) return;
            showModel = value;
            NotifyOfPropertyChange(() => ShowModel);
            RefreshDisplay();
        }
    }

    private readonly OxyColor acceptedPointsColor = OxyColors.Red;
    private readonly OxyColor rejectedPointsColor = OxyColors.Blue;
    private readonly OxyColor modelPointsColor = OxyColors.Yellow;
    private bool showAcceptedPoints = true;
    private bool showRejectedPoints = true;
    private bool showModelPoints = true;
    private bool showModel = false;

    #region UI Bound

    public PlotModel CrossSectionPlotModel { get; private set; }

    public LogSection? CurrentSection
    {
        get => currentSection;
        set
        {
            if (value == currentSection)
            {
                return;
            }
            currentSection = value;
            RefreshDisplay();
        }
    }

    #endregion

    #region Lifecycle

    public CrossSectionViewModel(ILogger logger)
    {
        Logger = logger;
        SetupPlot();
        viewMode = Mode.ModeSection;
    }

    #endregion

    #region Private Methods

    private void RefreshDisplay()
    {
        if (CurrentSection == null)
        {
            CrossSectionPlotModel.Series.Clear();
            CrossSectionPlotModel.Annotations.Clear();
            CrossSectionPlotModel.InvalidatePlot(true);
            return;
        }

        RefreshSeries();
        RefreshAnnotations();
        CrossSectionPlotModel.InvalidatePlot(true);
    }

    private void RefreshAnnotations()
    {
        if (CurrentSection == null)
        {
            return;
        }

    }

    private void RefreshSeries()
    {
        CrossSectionPlotModel.Series.Clear();
        if (CurrentSection == null)
        {
            return;
        }

        if (ShowAcceptedPoints && ViewMode == Mode.ModeSection)
        {
            var s = new ScatterSeries()
            {
                Title = $"Accepted Pts:",
                MarkerStroke = acceptedPointsColor,
                MarkerType = MarkerType.Cross,
                MarkerSize = 0.5,
                MarkerFill = acceptedPointsColor,

            };
            s.Points.AddRange(CurrentSection.AcceptedPoints.ToScatterPoints());
            CrossSectionPlotModel.Series.Add(s);
        }
        if (ShowRejectedPoints && ViewMode == Mode.ModeSection)
        {
            var s = new ScatterSeries()
            {
                Title = $"Rejected Pts:",
                MarkerStroke = rejectedPointsColor,
                MarkerType = MarkerType.Circle,
                MarkerSize = 1.0,
                MarkerFill = rejectedPointsColor,

            };
            s.Points.AddRange(CurrentSection.RejectedPoints.ToScatterPoints());
            CrossSectionPlotModel.Series.Add(s);
        }
        if (ShowModelPoints && ViewMode == Mode.ModeSection)
        {
            var s = new ScatterSeries()
            {
                Title = $"Model Pts:",
                MarkerStroke = modelPointsColor,
                MarkerType = MarkerType.Circle,
                MarkerSize = 2.0,
                MarkerFill = modelPointsColor,

            };
            s.Points.AddRange(CurrentSection.ModeledProfile.ToScatterPoints());
            CrossSectionPlotModel.Series.Add(s);
        }

    }

    private void SetupPlot()
    {
        CrossSectionPlotModel  = new PlotModel
        {
            PlotType = PlotType.Cartesian,
            Background = OxyColorsForStyle.PlotBackgroundColor,
            PlotAreaBorderColor = OxyColorsForStyle.PlotAreaBorderColor,
            PlotAreaBorderThickness = new OxyThickness(0),
            PlotMargins = new OxyThickness(-10)
        };
        CrossSectionPlotModel.Legends.Add(new Legend
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
        CrossSectionPlotModel.Axes.Add(columnAxis);

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
        CrossSectionPlotModel.Axes.Add(rowAxis);
    }

    #endregion
}
