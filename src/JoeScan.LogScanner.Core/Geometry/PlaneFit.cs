using System.Runtime.InteropServices;
using System.Security;

namespace JoeScan.LogScanner.Core.Geometry
{
    public static class PlaneFit
    {
        [SuppressUnmanagedCodeSecurity]

        [DllImport("BestPlaneFit", CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true, SetLastError = false)]
        public static extern bool getBestFitPlane(
            uint count,
            [In] double[] points,
            uint stride,
            [In] double[] weights,
            uint vstride,
            [In, Out] double[] plane);
    }
}
