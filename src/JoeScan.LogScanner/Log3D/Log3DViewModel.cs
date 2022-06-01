using Caliburn.Micro;
using HelixToolkit.Wpf;
using JoeScan.LogScanner.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace JoeScan.LogScanner.Log3D;

public class Log3DViewModel : Screen
{
    private readonly ActionBlock<RawLog> displayActionBlock;
    public LogScannerEngine Engine { get; }
    public HelixViewport3D? Viewport { get; set; }

    private Dictionary<uint, PointsVisual3D> pointsVisuals = new Dictionary<uint, PointsVisual3D>();
    int count = 0;
    private Rect3D extents;

    public Log3DViewModel(LogScannerEngine engine)
    {
        Engine = engine;
        displayActionBlock = new ActionBlock<RawLog>(RefreshDisplay, new ExecutionDataflowBlockOptions
        {
            TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
        });
        engine.RawLogs.LinkTo(displayActionBlock);

    }

    private void RefreshDisplay(RawLog p)
    {
        Viewport!.Children.Clear();
        long firstEncoderVal = p.ProfileData.First().EncoderValues[0];
        var pointsDict = new Dictionary<uint, Point3DCollection>();
        extents = Rect3D.Empty;
        double minX = 0, minY = 0, maxX = 0, maxY = 0;
        foreach (var profile in p.ProfileData)
        {
            uint id = profile.ScanHeadId;
            if (!pointsVisuals.ContainsKey(id))
            {
                //var col = ColorDefinitions.ColorForCableId(id);
                var col = Colors.SandyBrown;
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
            p.ProfileData[^1].EncoderValues[0] - firstEncoderVal);
        foreach (var id in pointsVisuals.Keys)
        {
            pointsVisuals[id].Points = pointsDict[id];
            Viewport.Children.Add(pointsVisuals[id]);
        }
        pointsVisuals.Clear();
        // ZoomToFit();

    }

    protected override void OnViewAttached(object view, object context)
    {
        if (view is Log3DView lv)
        {
            Viewport = lv.Viewport;

        }
    }

    public void ZoomToFit()
    {
        Viewport.ZoomExtents(extents, 300);
    }
}
