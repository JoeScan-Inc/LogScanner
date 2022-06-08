using Caliburn.Micro;
using HelixToolkit.Wpf;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace JoeScan.LogScanner.Log3D;

public class Log3DViewModel : Screen
{
    #region Injected Properties

    public LogScannerEngine Engine { get; }
    public ILogger Logger { get; }

    #endregion

    public HelixViewport3D? Viewport { get; set; }

    private Dictionary<uint, PointsVisual3D> pointsVisuals = new Dictionary<uint, PointsVisual3D>();
    int count = 0;
    private Rect3D extents;
    private PointColorMode selectedColorMode = PointColorMode.ByCableId;

    public enum PointColorMode
    {
        ByCableId,
        ByIntensity,
        Flat
    }

    public IObservableCollection<KeyValuePair<PointColorMode, string>> ColorMode { get; }
        = new BindableCollection<KeyValuePair<PointColorMode, string>>()
        {
            new KeyValuePair<PointColorMode, string>(PointColorMode.ByCableId, "By Cable ID"),
            // new KeyValuePair<PointColorMode, string>(PointColorMode.ByIntensity, "By Laser Intensity"),
            new KeyValuePair<PointColorMode, string>(PointColorMode.Flat, "Flat")
        };

    public PointColorMode SelectedColorMode
    {
        get => selectedColorMode;
        set  {
            if (value != selectedColorMode)
            {
                selectedColorMode = value;
                RefreshDisplay();
            }
        }
    }

    #region Lifecycle

    public Log3DViewModel(LogScannerEngine engine, ILogger logger)
    {
        Engine = engine;
        Logger = logger;
        var displayActionBlock = new ActionBlock<RawLog>(StoreAndDisplay, new ExecutionDataflowBlockOptions
        {
            TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
        });
        engine.RawLogs.LinkTo(displayActionBlock);

    }

    private void StoreAndDisplay(RawLog p)
    {
        CurrentLog = p;
        RefreshDisplay();
    }

    private RawLog? CurrentLog { get; set; }

    #endregion

    private void RefreshDisplay()
    {
        Viewport!.Children.Clear();
        if (CurrentLog == null)
            return;
        long firstEncoderVal = CurrentLog.ProfileData.First().EncoderValues[0];
        var pointsDict = new Dictionary<uint, Point3DCollection>();
        extents = Rect3D.Empty;
        double minX = 0, minY = 0, maxX = 0, maxY = 0;
        foreach (var profile in CurrentLog.ProfileData)
        {
            uint id = profile.ScanHeadId;
            if (!pointsVisuals.ContainsKey(id))
            {
                var col = Colors.SandyBrown;
                switch (SelectedColorMode)
                {
                    case PointColorMode.ByCableId:
                        col = ColorDefinitions.ColorForCableId(id);
                        break;
                    case PointColorMode.ByIntensity:
                        break;
                   
                }
                pointsVisuals[id] =
                    new PointsVisual3D() { Color = col, Size = 1 };
                pointsDict[id] = new Point3DCollection(10000);
            }


            foreach (var p2D in profile.Data)
            {
                if (p2D.X < minX)
                    minX = p2D.X;
                if (p2D.Y < minY)
                    minY = p2D.Y;
                if (p2D.X > maxX)
                    maxX = p2D.X;
                if (p2D.Y > maxY)
                    maxY = p2D.Y;

                pointsDict[id].Add(new Point3D(p2D.X, p2D.Y, (profile.EncoderValues[0] - firstEncoderVal)));
            }
        }
        //TODO: I don't think we need extents
        extents = new Rect3D(minX, minY, 0, maxX - minX, maxY - minY,
            CurrentLog.ProfileData[^1].EncoderValues[0] - firstEncoderVal);
        foreach (var id in pointsVisuals.Keys)
        {
            pointsVisuals[id].Points = pointsDict[id];
            Viewport.Children.Add(pointsVisuals[id]);
        }
        pointsVisuals.Clear();
        // ZoomToFit();

    }

    #region IViewAware Implementation

    protected override void OnViewAttached(object view, object context)
    {
        if (view is Log3DView lv)
        {
            Viewport = lv.Viewport;
            Viewport.Orthographic = true;

        }
    }

    #endregion

    public void ZoomToFit()
    {
        Viewport.ZoomExtents(extents, 300);
    }
}
