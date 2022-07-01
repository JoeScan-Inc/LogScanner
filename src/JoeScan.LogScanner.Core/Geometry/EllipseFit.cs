using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Drawing;

namespace JoeScan.LogScanner.Core.Geometry   
{
    public static class EllipseFit
    {
        static EllipseFit()
        {
            // force Math.NET to use the managed implementation
            Control.UseManaged();    
        }

        public static PointF[] MakeEllipseSection(double a, double b, double angle, double x0, double y0, double top, double bottom, int pointCount)
        {
            PointF[] points = new PointF[pointCount];
            
            double rotSin = Math.Sin(angle);
            double rotCos = Math.Cos(angle);

            double step = Math.PI * 2 / (pointCount-1);

            // Convert from a*sin(x)+ b*cos(x) => Acos(x - C)
            double Ax = Math.Sqrt(Math.Pow(a * rotCos,2.0) + Math.Pow(b*rotSin, 2.0));
            double Cx = Math.Atan2(-b * rotSin, a * rotCos);

            double Ay = Math.Sqrt(Math.Pow(a * rotSin, 2.0) + Math.Pow(b * rotCos, 2.0));
            double Cy = Math.Atan2(b * rotCos, a * rotSin);

            for (int i = 0; i < pointCount; i++)
            {
                double theta = i * step;
                points[i].X = (float)( Ax * Math.Cos(theta - Cx) + x0 );
                points[i].Y = (float)Math.Max(Math.Min(( Ay * Math.Cos(theta - Cy) + y0 ),top),bottom);
            }

            return points;
        }


        public static PointF[] MakeEllipseSection(double a, double b, double angle, double x0, double y0, int pointCount)
        {
            PointF[] points = new PointF[pointCount];

            double rotSin = Math.Sin(angle);
            double rotCos = Math.Cos(angle);

            double step = Math.PI * 2 / (pointCount - 1);

            // Convert from a*sin(x)+ b*cos(x) => Acos(x - C)
            double Ax = Math.Sqrt(Math.Pow(a * rotCos, 2.0) + Math.Pow(b * rotSin, 2.0));
            double Cx = Math.Atan2(-b * rotSin, a * rotCos);

            double Ay = Math.Sqrt(Math.Pow(a * rotSin, 2.0) + Math.Pow(b * rotCos, 2.0));
            double Cy = Math.Atan2(b * rotCos, a * rotSin);

            for (int i = 0; i < pointCount; i++)
            {
                double theta = i * step;
                points[i].X = (float)(Ax * Math.Cos(theta - Cx) + x0);
                points[i].Y = (float) (Ay*Math.Cos(theta - Cy) + y0);
            }

            return points;
        }
        
        internal static double MaxDistance(double xCenter, double yCenter, double angle, double a, double b)
        {
            double max = 0.0;
            double rotSin = Math.Sin(angle);
            double rotCos = Math.Cos(angle);

            // Convert from a*sin(x)+ b*cos(x) => Acos(x - C)
            double Ax = Math.Sqrt(Math.Pow(a * rotCos, 2.0) + Math.Pow(b * rotSin, 2.0));
            double Cx = Math.Atan2(-b * rotSin, a * rotCos);
            double Ay = Math.Sqrt(Math.Pow(a * rotSin, 2.0) + Math.Pow(b * rotCos, 2.0));
            double Cy = Math.Atan2(b * rotCos, a * rotSin);          
            // numerical approximation of the maximal distance between the center of the coordinate system and the farthest point of an ellipse
            for (double theta = 0.0; theta < Math.PI * 2; theta += Math.PI / 100)
            {
                double x = Ax * Math.Cos(theta - Cx) + xCenter;
                double y = Ay * Math.Cos(theta - Cy) + yCenter;
                double offset = Math.Sqrt(x * x + y * y);
                if (offset > max)
                {
                    max = offset;
                }
            }
            return max;
        }

        // This is a managed implementation of the ellipse fit algorithm 
        // that directly replaces the EllipseFitDirectMKL code. No native MKL libraries are needed 
        // anymore and have been removed from the build.
        // DO NOT use the Math.NET MKL provider with this code (Control.UseNativeMKL()), the EigenValueDecomposition (Evd()) 
        // gives wrong results, presumably due to a bug in Math.NET.

        public static double[] EllipseFitMathNet(float[][] input)
        {
            var count = input[0].Length;
            var x2Arr = new double[count];
            var xyArr = new double[count];
            var y2Arr = new double[count];
            var xArr = new double[count];
            var yArr = new double[count];
            var uv = new double[count];
            for (int i = 0; i < count; i++)
            {
                x2Arr[i] = input[0][i] * input[0][i];
                y2Arr[i] = input[1][i] * input[1][i];
                xyArr[i] = input[0][i] * input[1][i];
                xArr[i] = input[0][i];
                yArr[i] = input[1][i];
                uv[i] = 1.0F;
            }
            Matrix<double> d1 = DenseMatrix.OfColumnArrays(x2Arr, xyArr, y2Arr);
            Matrix<double> d2 = DenseMatrix.OfColumnArrays(xArr, yArr, uv);

            var s1 = d1.TransposeThisAndMultiply(d1);
            //            Console.WriteLine(s1.ToMatrixString());
            var s2 = d1.TransposeThisAndMultiply(d2);
            //            Console.WriteLine(s2.ToMatrixString());
            var s3 = d2.TransposeThisAndMultiply(d2);
            //            Console.WriteLine(s3.ToMatrixString());

            var c = DenseMatrix.OfColumnMajor(3, 3, new[]
            {
                0.0, 0.0F, 2.0F,
                0.0F, -1.0F, 0.0F,
                2.0F, 0.0F, 0.0F
            });
            var t = s3.Inverse().Multiply(s2.Transpose()).Multiply(-1.0F);
            var m1 = s1.Add(s2.Multiply(t));
            //            Console.WriteLine(m1.ToMatrixString());
            var m2 = c.Inverse().Multiply(m1);
            //            Console.WriteLine(m2.ToMatrixString("0"));
            var evd = m2.Evd();
            var evec = evd.EigenVectors;
            //            Console.WriteLine(evec.ToMatrixString());

            Matrix<double> a1 = new DenseMatrix(3, 1);
            for (int i = 0; i < 3; i++)
            {
                double cond = 4 * evec[0, i] * evec[2, i] - Math.Pow(evec[1, i], 2);
                if (cond > 0)
                {
                    a1 = evec.SubMatrix(0, 3, i, 1);
                }
            }
            var a = new DenseMatrix(6, 1);
            a.SetSubMatrix(0, 3, 0, 1, a1);
            a.SetSubMatrix(3, 3, 0, 1, t.Multiply(a1));
            return a.Column(0).ToArray();
        }



        /// <summary>
        /// Convert from Quartic Coef. to a parametric ellipse
        /// 
        /// </summary>
        /// <param name="A">Quartic Coef. : A[0]*x^2 + A[1]*X*Y + A[2]*Y^2 + A[3]*x + A[4]*y +A[5] = 0 </param>
        /// <returns>{radiusA,radiusB,angle,x0,y0}</returns>
        public static double[] QuarticToEllipse(double[] A)
        {
            if (A == null)
                throw new ArgumentNullException("A");

            if (A.Length != 6)
                throw new ArgumentException("Array must contain 6 elements", "A");
            
            double a = A[0];
            double b = A[1] / 2;
            double c = A[2];
            double d = A[3] / 2;
            double f = A[4] / 2;
            double g = A[5];

            double x0 = (c * d - b * f) / (b * b - a * c);
            double y0 = (a * f - b * d) / (b * b - a * c);

            double ra = Math.Sqrt(2 * (a * f * f + c * d * d + g * b * b - 2 * b * d * f - a * c * g) / 
                ((b * b - a * c) * (Math.Sqrt((a - c) * (a - c) + 4 * b * b) - (a + c))));
            double rb = Math.Sqrt(2 * (a * f * f + c * d * d + g * b * b - 2 * b * d * f - a * c * g) / 
                ((b * b - a * c) * (-Math.Sqrt((a - c) * (a - c) + 4 * b * b) - (a + c))));

            //double angle = Math.Atan2(a-c,2*b);

            double angle = 0;
            if (a >= c)
                angle = Math.PI / 2.0;

            if (b != 0)
                angle += 1.0 / 2.0 * Math.Atan(2 * b / (a - c));

            angle = (angle + Math.PI) % Math.PI;
        
            return new double[] {ra, rb, angle, x0, y0};
        }

        /// <summary>
        /// Fill an array with the error of that point from given ellipse.
        /// 
        /// Ideally, this would be the geometric [sqrt(x^2 + y^2)] distance, but it is a bit hard to calculate, so
        /// instead this uses distance along the ray from the center through the point.
        /// This will be reasonably accurate for substatially circular ellipses.
        /// </summary>
        /// <param name="ra">Radius A</param>
        /// <param name="rb">Radius B</param>
        /// <param name="angle">Angle of Radius A from level</param>
        /// <param name="x0">X center</param>
        /// <param name="y0">Y center</param>
        /// <param name="pts">pts[0][] == x[] & pts[1][] == y[]</param>
        /// <returns>estimated error in distance from center</returns>
        public static float[] PointToEllipseError(double ra, double rb, double angle, double x0, double y0, float[][] pts)
        {

            double rotSin = Math.Sin(angle);
            double rotCos = Math.Cos(angle);

            float[] errors = new float[pts[0].Length];

            for (int i = 0; i < pts[0].Length; i++)
            {
                double x = pts[0][i] - x0;
                double y = pts[1][i] - y0;

                double centerToPointDistance = Math.Sqrt(x * x + y * y);
                double centerToPointAngle = Math.Atan2(y, x);

                double ellipseRadiusAtAngle = (Math.Cos(2 * centerToPointAngle - angle) + 1.0) / 2.0 * (ra - rb) + rb;

                errors[i] =  (float) Math.Abs(centerToPointDistance - ellipseRadiusAtAngle);
            }
            return errors;
        }

        public static float[] PointToEllipseError(double[] v, float[][] pts)
        {
            return PointToEllipseError(v[0], v[1], v[2], v[3], v[4], pts);
        }

        public static float LeastSquaresEllipseError(double ra, double rb, double angle, double x0, double y0, float[][] pts)
        {
            double squareError = 0;

            for (int i = 0; i < pts[0].Length; i++)
            {
                double x = pts[0][i] - x0;
                double y = pts[1][i] - y0;

                double centerToPointDistance = Math.Sqrt(x * x + y * y);
                double centerToPointAngle = Math.Atan2(y, x);

                double ellipseRadiusAtAngle = (Math.Cos(2 * centerToPointAngle - angle) + 1.0) / 2.0 * (ra - rb) + rb;

                double error = (centerToPointDistance - ellipseRadiusAtAngle);
                squareError += error * error;
            }
            return (float)Math.Sqrt(squareError/pts[0].Length);
        }

       

        public static Ellipse EllipseFitDirect(float[][] pts)
        {
            return new Ellipse(EllipseFit.QuarticToEllipse(EllipseFit.EllipseFitMathNet(pts)));
        }
    }
}
