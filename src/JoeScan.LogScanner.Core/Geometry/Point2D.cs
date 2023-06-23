namespace JoeScan.LogScanner.Core.Geometry
{
    public record Point2D(float X, float Y, byte B = 0) : IEquatable<Point2D?>
    {
        public bool IsValid { get; set; } = true;


        public double Distance(Point2D p2)
        {
            return Math.Sqrt((X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y));
        }

        public virtual bool Equals(Point2D? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return X.Equals(other.X) && Y.Equals(other.Y) && B == other.B;
        }
       

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, B);
        }
    }
}
