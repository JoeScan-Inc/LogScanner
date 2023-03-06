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
using AxisPosition = OxyPlot.Axes.AxisPosition;
using LegendPosition = OxyPlot.Legends.LegendPosition;

namespace RawViewer.Shell;

public class ShellViewModel : Screen
{
    public ILogger Logger { get; }
    private readonly IDialogService dialogService;
    private readonly IRawViewerConfig config;
    private Profile? selectedProfile;
    public Dictionary<uint, ObservableCollection<Profile>> ProfilesDict { get; set; } = new();
    public ObservableCollection<ISeries> Series { get; set; } = new ObservableCollection<ISeries>();
    public ObservableCollection<Profile> Profiles { get; set; } = new();

    public Profile? SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (Equals(value, selectedProfile))
                return;
            selectedProfile = value;
            NotifyOfPropertyChange(() => SelectedProfile);
        }
    }

    public PlotModel? LiveView { get; private set; }

    public ShellViewModel(IDialogService dialogService, IRawViewerConfig config, ILogger logger)
    {
        Logger = logger;
        this.dialogService = dialogService;
        this.config = config;
        SetupPlotModel();
    }

    public void Load()
    {
        var initialDirectory = config.LastFileBrowserLocation?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var openFileDialogSettings = new OpenFileDialogSettings()
        {
            Title = "Open raw log archive file",
            InitialDirectory = initialDirectory,
            Filter = $"Raw LogScanner Recording (*.raw)|*.raw|All Files (*.*)|*.*",
            CheckFileExists = true
        };

        bool? success = dialogService.ShowOpenFileDialog(this, openFileDialogSettings);
        if (success == true)
        {
            FillBuffer(openFileDialogSettings.FileName);
            config.LastFileBrowserLocation = Path.GetDirectoryName(openFileDialogSettings.FileName);
        }
    }

    private void FillBuffer(string fileName)
    {
        try
        {
            ProfilesDict.Clear();
            using var fs = new FileStream(fileName, FileMode.Open);

            using var br = new BinaryReader(fs);
            while (true)
            {

                var p = ProfileReaderWriter.Read(br);
              
                if (!ProfilesDict.ContainsKey(p.ScanHeadId))
                {
                    ProfilesDict[p.ScanHeadId] = new ObservableCollection<Profile>();
                }
                ProfilesDict[p.ScanHeadId].Add(p);
                Profiles.Add(p);
            }
        }
        catch (EndOfStreamException)
        {
            //ignore, eof
        }
        catch (Exception e)
        {
            // anything else
            Logger.Fatal(e, $"Failed to open/read replay file: {fileName}");
            throw;
        }

        foreach (var head in ProfilesDict.Keys)
        {
            Series.Add(new ScatterSeries<Profile>()
            {
                Values = ProfilesDict[head],
                Mapping = (profile, point) =>
                {
                   
                    point.PrimaryValue = (float)profile.Data.Length;
                    point.SecondaryValue = profile.EncoderValues[0];
                },
                Stroke = new SolidColorPaint(SKColor.Parse(ColorDefinitions.ColorForCableId(head).ToString())) { StrokeThickness = 1 },
                Fill = null,
                GeometrySize = 1
            });
        }

        // var timeInHeadDiff = ProfilesDict[0][^1].TimeStampNs - ProfilesDict[0][0].TimeStampNs;
        // var encDiff = ProfilesDict[0][^1].EncoderValues[0] - ProfilesDict[0][0].EncoderValues[0];

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

}
