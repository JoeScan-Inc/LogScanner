namespace JoeScan.LogScanner.Core.Geometry
{
    public record Point2D
    {

        public double X { get; init; }
        public double Y { get; init; }
        public double B { get; init; }

        public Point2D() { }
        public Point2D(double x, double y, double b)
        {
            X = x;
            Y = y;
            B = b;
        }

        public double Distance(Point2D p2)
        {
            return Math.Sqrt((X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y));
        }
    }
}
