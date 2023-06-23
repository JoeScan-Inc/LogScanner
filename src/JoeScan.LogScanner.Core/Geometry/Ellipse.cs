namespace JoeScan.LogScanner.Core.Geometry
{
    /// <summary>
    /// Simple utility class to hold p1arameters of an ellipse in cartesion form.
    /// </summary>
    public class Ellipse
    {
        public double A { get; private set; }
        public double Area { get; private set; }
        public double B { get; private set; }
        public double Theta { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }

        public Ellipse(double[] p)
        {
            if (p[0] > p[1])
            {
                A = p[0];
                B = p[1];
                Theta = p[2];
            }
            else
            {
                A = p[1];
                B = p[0];
                Theta = (p[2] + Math.PI / 2) % Math.PI;
            }
            Area = Math.PI * A * B;
            X = p[3];
            Y = p[4];
        }

        public double PointToEllipseError(float px, float py)
        {
            double x = px - X;
            double y = py - Y;

            double centerToPointDistance = Math.Sqrt(x * x + y * y);
            double phi = Math.Atan2(y, x);

            double ellipseRadiusAtAngle = (A * B) /
                                          Math.Sqrt((B * Math.Cos(Theta - phi)) * (B * Math.Cos(Theta - phi)) +
                                                    (A * Math.Sin(Theta - phi)) * (A * Math.Sin(Theta - phi)));
            return Math.Abs(centerToPointDistance - ellipseRadiusAtAngle);
        }


        public Point2D[] CreatePoints(int numPoints)
        {
            var l = new List<Point2D>(numPoints);
            var inc = (Math.PI * 2) / numPoints;
            var angle = 0.0;
            for (int i = 0; i < numPoints; i++)
            {
                l.Add(new Point2D((float)(A * Math.Cos(angle) * Math.Cos(Theta) - B * Math.Sin(angle) * Math.Sin(Theta) + X),
                    (float)(B * Math.Sin(angle) * Math.Cos(Theta) + A * Math.Cos(angle) * Math.Sin(Theta) + Y), 0));
                angle += inc;
            }
            return l.ToArray();
        }

        public Point2D[] IntersectWithLine(double m, double b1)
        {
            // see http://quickcalcbasic.com/ellipse%20line%20intersection.pdf for formulas
            double v = B;
            double h = A;
            var bb1 = b1 + (m * X - Y); // for non-centered ellipse
            double a = v * v * Math.Cos(Theta) * Math.Cos(Theta) +
                2 * v * v * m * Math.Cos(Theta) * Math.Sin(Theta) +
                       v * v * m * m * Math.Sin(Theta) * Math.Sin(Theta) +
                       h * h * m * m * Math.Cos(Theta) * Math.Cos(Theta) -
                       2 * h * h * m * Math.Cos(Theta) * Math.Sin(Theta) +
                       h * h * Math.Sin(Theta) * Math.Sin(Theta);
            double b = 2 * v * v * bb1 * Math.Cos(Theta) * Math.Sin(Theta) + 2 * v * v * m * bb1 * Math.Sin(Theta) * Math.Sin(Theta) +
                       2 * h * h * m * bb1 * Math.Cos(Theta) * Math.Cos(Theta)
                       - 2 * h * h * bb1 * Math.Cos(Theta) * Math.Sin(Theta);
            double c = bb1 * bb1 * (v * v * Math.Sin(Theta) * Math.Sin(Theta) + h * h * Math.Cos(Theta) * Math.Cos(Theta)) -
                       h * h * v * v;
            var res = new Point2D[2];
            var x1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a) + X;
            var y1 = m * x1 + b1;
            res[0] = new Point2D((float)x1, (float)y1, 0);
            var x2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a) + X;
            var y2 = m * x2 + b1;
            res[1] = new Point2D((float)x2, (float)y2, 0);
            return res;
        }

    }
}
