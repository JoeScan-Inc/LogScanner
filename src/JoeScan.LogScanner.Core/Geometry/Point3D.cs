namespace JoeScan.LogScanner.Core.Geometry
{
    public record Point3D
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }
        public double B { get; init; }

        public Point3D(double x, double y, double z, double brightness)
        {
            X = x;
            Y = y;
            Z = z;
            B = brightness;
        }

        public Point3D(Point2D p2, float z)
        {
            X = p2.X;
            Y = p2.Y;
            Z = z;
            B = p2.B;
        }
    }
}
