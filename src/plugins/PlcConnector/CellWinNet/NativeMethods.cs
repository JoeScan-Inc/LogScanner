using System.Runtime.InteropServices;
using System.Security;

namespace CellWinNet;

[SuppressUnmanagedCodeSecurity]
public sealed class NativeMethods
{
    private NativeMethods() { }

    public delegate void Callback(string text);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_connect(string ipAddress, int device_number_cpu, int device_number_rack);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern void js_disconnect();

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_read_tag_8(string tagname, [Out] out byte c);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_read_tag_16(string tagname, [Out] out short c);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_read_tag_32(string tagname, [Out] out int i);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_write_tag_8(string tagname, [In] byte i);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_write_tag_16(string tagname, [In] short i);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_write_tag_32(string tagname, [In] int i);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern int js_write_tag_array_32(string tagname, [In] int[] i, int count);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern void setDPrintCallback(Callback fn);

    [DllImport("cell-win", CallingConvention = CallingConvention.Cdecl, ExactSpelling = false, SetLastError = false)]
    public static extern void setDebugLevel(int level);

}

