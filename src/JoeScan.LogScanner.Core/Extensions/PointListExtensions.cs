using JoeScan.LogScanner.Core.Geometry;


namespace JoeScan.LogScanner.Core.Extensions
{
    public static class PointListExtensions
    {
        public static float[][] ToFloatArray(this IList<Point2D> pts)
        {
            float[][] ptArray = new float[3][];

            ptArray[0] = new float[pts.Count];
            ptArray[1] = new float[pts.Count];
            ptArray[2] = new float[pts.Count];

            for (int i = 0; i < pts.Count; i++)
            {
                ptArray[0][i] = (float)pts[i].X;
                ptArray[1][i] = (float)pts[i].Y;
                ptArray[2][i] = (float)pts[i].B;
            }
            return ptArray;
        }
        public static float[][] ToFloatArrayA(this Point2D[] pts)
        {
            float[][] ptArray = new float[3][];

            ptArray[0] = new float[pts.Length];
            ptArray[1] = new float[pts.Length];
            ptArray[2] = new float[pts.Length];

            for (int i = 0; i < pts.Length; i++)
            {
                ptArray[0][i] = (float)pts[i].X;
                ptArray[1][i] = (float)pts[i].Y;
                ptArray[2][i] = (float)pts[i].B;
            }
            return ptArray;
        }


    }
}
