namespace JoeScan.LogScanner.Core.Geometry
{
    public static class LineFit
    {
        public static void LeastSquaresFitLinear(IEnumerable<Point2D> points, out double slope, out double offset, out double rmse)
        {
            //Gives best fit of data to line Y = slope * C + offset
            int i = 0;
            
            double x1 = 0.0;
            double y1 = 0.0;
            double xy = 0.0;
            double x2 = 0.0;

            foreach (var p in  points )
            {
                x1 = x1 + p.X;
                y1 = y1 + p.Y;
                xy = xy + p.X * p.Y;
                x2 = x2 + p.X * p.X;
                i++;
            }

            double J = (i * x2) - (x1 * x1);
            if (J != 0.0)
            {
                slope = ((i * xy) - (x1 * y1)) / J;
                slope = Math.Floor(1.0E3 * slope + 0.5) / 1.0E3;
                offset = ((y1 * x2) - (x1 * xy)) / J;
                offset = Math.Floor(1.0E3 * offset + 0.5) / 1.0E3;
            }
            else
            {
                slope = 0;
                offset = 0;
            }
            double mse = 0.0;
            foreach (Point2D p in points)
            {
                mse += ((slope * p.X + offset) - p.Y) * ((slope * p.X + offset) - p.Y);
            }
            rmse = Math.Sqrt(mse / (i - 2));
        }

        public static void LeastSquaresFitLinear(Point2D[] points, out double slope, out double offset, out double rmse)
        {
            //Gives best fit of data to line Y = slope * C + offset
            double x1, y1, xy, x2, J;
            int i;
            int numPoints = points.Length;

            x1 = 0.0;
            y1 = 0.0;
            xy = 0.0;
            x2 = 0.0;

            for (i = 0; i < numPoints; i++)
            {
                x1 = x1 + points[i].X;
                y1 = y1 + points[i].Y;
                xy = xy + points[i].X * points[i].Y;
                x2 = x2 + points[i].X * points[i].X;
            }

            J = ((double)numPoints * x2) - (x1 * x1);
            if (J != 0.0)
            {
                slope = (((double)numPoints * xy) - (x1 * y1)) / J;
                offset = ((y1 * x2) - (x1 * xy)) / J;
            }
            else
            {
                slope = 0;
                offset = 0;
            }
            double mse = 0.0;
            foreach (Point2D p in points)
            {
                mse += ((slope * p.X + offset) - p.Y) * ((slope * p.X + offset) - p.Y);
            }
            rmse = Math.Sqrt(mse / (points.Length - 2));
        }

    }
}
