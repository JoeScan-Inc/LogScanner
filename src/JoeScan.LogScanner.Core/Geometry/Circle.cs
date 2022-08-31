using JoeScan.LogScanner.Core.Helpers;

namespace JoeScan.LogScanner.Core.Geometry
{
    public class Circle
    {
        public double x;
        public double y;
        public double r;

        public static Point2D[] MakeSectionPoints(Point2D center, double diameter,
            double startAngleRad, double endAngleRad, int numPts, bool addNoise)
        {
            var pts = new Point2D[numPts];
            var dT = (endAngleRad - startAngleRad) / numPts;
            
            for (int i = 0; i < numPts; i++)
            {
                var t = startAngleRad + i * dT;
                var radius = addNoise ? diameter/2 + RandomHelper.GetRandomDouble() / 50.0 * diameter / 2:diameter/2;
                pts[i] = new Point2D(center.X + Math.Cos(t) * radius,
                    center.Y + Math.Sin(t) * radius,
                    100);


            }

            return pts;

        }
    }
}
