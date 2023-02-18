using System.Runtime.InteropServices;
using System;
using System.Text;

namespace JoeScan.LogScanner.LogReview.Helpers;

public static class WinApi
{
    public const int HWND_BROADCAST = 0xffff;
    public const int SW_SHOWNORMAL = 1;
    public const int WM_USER = 0x400;
    public const int WM_COPYDATA = 0x4A;

    // ReSharper disable InconsistentNaming
    public enum MoveFileFlags
    {

        MOVEFILE_REPLACE_EXISTING = 1,
        MOVEFILE_COPY_ALLOWED = 2,
        MOVEFILE_DELAY_UNTIL_REBOOT = 4,
        MOVEFILE_WRITE_THROUGH = 8
    } // ReSharper restore InconsistentNaming

    public static int RegisterWindowMessage(string format, params object[] args)
    {
        string message = String.Format(format, args);
        return RegisterWindowMessage(message);
    }

    //Used for WM_COPYDATA for string messages

    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("User32.dll")]
    private static extern int RegisterWindowMessage(string lpString);

    [DllImport("User32.dll", EntryPoint = "FindWindow")]
    public static extern Int32 FindWindow(String lpClassName, String lpWindowName);

    //For use with WM_COPYDATA and COPYDATASTRUCT
    [DllImport("User32.dll", EntryPoint = "SendMessage")]
    public static extern int SendMessage(int hWnd, int Msg, int wParam, ref CopyDataStruct lParam);

    //For use with WM_COPYDATA and COPYDATASTRUCT
    [DllImport("User32.dll", EntryPoint = "PostMessage")]
    public static extern int PostMessage(int hWnd, int Msg, int wParam, ref CopyDataStruct lParam);

    [DllImport("User32.dll", EntryPoint = "SendMessage")]
    public static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

    [DllImport("User32.dll", EntryPoint = "PostMessage")]
    public static extern int PostMessage(int hWnd, int Msg, int wParam, int lParam);

    [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
    public static extern bool SetForegroundWindow(int hWnd);

    [DllImport("kernel32.dll", EntryPoint = "MoveFileEx")]
    public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);


    public static void ShowToFront(IntPtr window)
    {
        ShowWindow(window, SW_SHOWNORMAL);
        SetForegroundWindow(window);
    }

    public struct CopyDataStruct
    {
        public int cbData;
        public IntPtr dwData;
        [MarshalAs(UnmanagedType.LPStr)] public string lpData;
    }

    public static int SendWindowsStringMessage(int hWnd, int wParam, string msg)
    {
        int result = 0;

        if (hWnd > 0)
        {
            byte[] sarr = Encoding.Default.GetBytes(msg);
            int len = sarr.Length;
            WinApi.CopyDataStruct cds;
            cds.dwData = (IntPtr)100;
            cds.lpData = msg;
            cds.cbData = len + 1;
            result = WinApi.SendMessage(hWnd, WinApi.WM_COPYDATA, wParam, ref cds);
        }

        return result;
    }
}

