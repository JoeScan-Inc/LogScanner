namespace JoeScan.LogScanner.Core.Geometry
{
    static public class CircleFit
    {
      

        public static double DistanceSquare(double x0, double y0, double x, double y)
        {
            return (x0 - x) * (x0 - x) + (y0 - y) * (y0 - y);
        }

        public static Circle FitThreePoints(int[] ind, List<Point2D> p)
        {
            // TODO switch inputpoints when on vertical line, and return null when collinear
            Point2D p1 = p[ind[0]];
            Point2D p2 = p[ind[1]];
            Point2D p3 = p[ind[2]];

            double ma = (p2.Y - p1.Y) / (p2.X - p1.X);
            double mb = (p3.Y - p2.Y) / (p3.X - p2.X);
            double x = (ma * mb * (p1.Y - p3.Y) + mb * (p1.X + p2.X)
                        - ma * (p2.X + p3.X)) / (2 * (mb - ma));
            double y = -(1 / ma) * (x - (p1.X + p2.X) / 2) + (p1.Y + p2.Y) / 2;
            return new Circle()
                       {
                           x = x,
                           y = y,
                           r = Math.Sqrt((x - p[ind[0]].X) * (x - p[ind[0]].X) +
                                         (y - p[ind[0]].Y) * (y - p[ind[0]].Y))
                       };

        }
    }

    
}
