using JoeScan.LogScanner.Core.Geometry;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace JoeScan.LogScanner.LogReview.Extensions;

public static class Point2DListExtensions
{
    public static IEnumerable<ScatterPoint> ToScatterPoints(this List<Point2D> pts)
    {
        return pts.Select(q=>new ScatterPoint(q.X,q.Y,double.NaN, q.B));
    }


}
