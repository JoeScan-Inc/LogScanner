namespace JoeScan.LogScanner.Core.Geometry
{
    /// <summary>
    /// The Plane3D class holds the parameters of the point-normal form of a plane and provides 
    /// various utility functions in a cartesian coordinate system.
    /// </summary>
    /// <remarks>
    /// The normal form of a plane can be described as <code>
    /// A * x + B * y + C * z + D = 0
    /// </code>
    /// See http://mathworld.wolfram.com/Plane.html for the formulas used.
    /// </remarks>
    public class Plane3D
    {

        /// <summary>
        /// Gets A
        /// </summary>
        /// <value>
        /// The parameter A in the plane equation
        /// </value>
        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }
        public double D { get; private set; }
        private double den;


        /// <summary>
        /// Initializes a new instance of the <see cref="Plane3D"/> class from input parameters.
        /// </summary>
        /// <param name="a">The parameter A in the plane equation</param>
        /// <param name="b">The parameter B in the plane equation</param>
        /// <param name="c">The parameter C in the plane equation</param>
        /// <param name="d">The parameter D in the plane equation</param>
        public Plane3D(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            den = Math.Sqrt(A * A + B * B + C * C);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Plane3D"/> class from an array of 4 double 
        /// values.
        /// </summary>
        /// <param name="p">The array of input values. Must be at least 4 elements log</param>
        public Plane3D(params double[] p)
            : this(p[0], p[1], p[2], p[3])
        {

        }

        /// <summary>
        /// Distances the specified p.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="Z">The z.</param>
        /// <returns></returns>
        public double Distance(Point2D p, double Z)
        {
            //return (Distance(new Point3D(p, (float) Z)));
            return (Math.Abs(p.X * A + p.Y * B + Z * C + D) / den);
        }


        public double Distance(Point3D p)
        {
            return (Math.Abs(p.X * A + p.Y * B + p.Z * C + D) / den);
        }

        public double GetY(Point3D p)
        {
            return (-A * p.X - C * p.Z - D) / B;
        }

        public double GetX(double y, double z)
        {
            if (Math.Abs(A - 0.0) < Double.Epsilon)
            {
                throw new DivideByZeroException();
            }
            return -(B * y + C * z + D) / A;
        }

        public double DihedralAngle(Plane3D p2)
        {
            var theta = (A * p2.A + B * p2.B + C * p2.C) / (Math.Sqrt(A * A + B * B + C * C) * Math.Sqrt(p2.A * p2.A + p2.B * p2.B + p2.C * p2.C));
            return Math.Acos(theta);
        }

        public double Slope()
        {
            return -A/B;
        }

        public double OffsetAtZ(double z)
        {
            return  -D/B + (C*z)/B;
        }

    }
}
