// Source: https://github.com/jimradford/superputty/blob/master/SuperPutty/Utils/NativeMethods.cs

using System;
using System.Runtime.InteropServices;

namespace NETworkManager.Utilities;

public class NativeMethods
{
    #region Win32 Constants

    public const int GWL_STYLE = -16;
    public const int WS_THICKFRAME = 0x00040000;
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_NOACTIVATE = 0x0010;
    public const int SWP_SHOWWINDOW = 0x0040;
    public const long WS_POPUP = 0x80000000L;
    public const long WS_CAPTION = 0x00C00000L;
    public const long WS_CHILD = 0x40000000L;
    public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    /// <summary>Places the window at the bottom of the Z order (behind all others).</summary>
    public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

    /// <summary>The value returned by CreateFile on failure.</summary>
    public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    #endregion

    #region Enum

    public enum WindowShowStyle : uint
    {
        Hide = 0,
        ShowNormal = 1,
        ShowMinimized = 2,
        ShowMaximized = 3,
        Maximize = 3,
        ShowNormalNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActivate = 7,
        ShowNoActivate = 8,
        Restore = 9,
        ShowDefault = 10,
        ForceMinimized = 11
    }

    public enum WM : uint
    {
        SYSCOMMAND = 0x0112
    }

    /// <summary>
    /// Controls how a thread hosts child windows with different DPI awareness contexts.
    /// Set to <see cref="DPI_HOSTING_BEHAVIOR_MIXED"/> before calling SetParent with a
    /// cross-process child window to enable DPI notification forwarding.
    /// Available on Windows 10 1803 (build 17134) and later.
    /// </summary>
    public enum DPI_HOSTING_BEHAVIOR
    {
        DPI_HOSTING_BEHAVIOR_INVALID = -1,
        DPI_HOSTING_BEHAVIOR_DEFAULT = 0,
        DPI_HOSTING_BEHAVIOR_MIXED = 1
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CONSOLE_FONT_INFOEX
    {
        public uint cbSize;
        public uint nFont;
        public COORD dwFontSize;
        public uint FontFamily;
        public uint FontWeight;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;
    }

    #endregion

    #region Pinvoke/Win32 Methods

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern long SetParent(IntPtr hWndChild, IntPtr hWndParent);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
    public static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644898%28v=vs.85%29.aspx
    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong));
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy,
        int wFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// Sets the DPI hosting behavior for windows created or reparented on the calling thread.
    /// Call with <see cref="DPI_HOSTING_BEHAVIOR.DPI_HOSTING_BEHAVIOR_MIXED"/> before
    /// SetParent to opt into mixed-DPI hosting and enable DPI notification routing for
    /// child windows. Returns the previous behavior so it can be restored.
    /// Windows 10 1803+ only.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern DPI_HOSTING_BEHAVIOR SetThreadDpiHostingBehavior(DPI_HOSTING_BEHAVIOR value);

    /// <summary>
    /// Returns the DPI (dots per inch) value for the monitor that contains the specified window.
    /// Returns 0 if the window handle is invalid. Available on Windows 10 version 1607+.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern uint GetDpiForWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeConsole();

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
        ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow,
        ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);

    #endregion

    #region Helpers

    /// <summary>
    /// Returns the physical-pixel bounding rectangle of the monitor that contains
    /// the specified window (nearest monitor if the window is off-screen).
    /// </summary>
    public static RECT GetMonitorBoundsForWindow(IntPtr hWnd)
    {
        var hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
        var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        GetMonitorInfo(hMonitor, ref mi);
        return mi.rcMonitor;
    }

    /// <summary>
    /// Attaches to <paramref name="processId"/>'s console and rescales its current font
    /// by <paramref name="scaleFactor"/> using <c>SetCurrentConsoleFontEx</c>.
    /// This is a cross-process-safe approach that bypasses WM_DPICHANGED message passing
    /// entirely. Works for any conhost-based console (PowerShell, cmd, etc.).
    /// </summary>
    public static void TryRescaleConsoleFont(uint processId, double scaleFactor)
    {
        if (Math.Abs(scaleFactor - 1.0) < 0.01)
            return;

        if (!AttachConsole(processId))
            return;

        const uint GENERIC_READ_WRITE = 0xC0000000u;
        const uint FILE_SHARE_READ_WRITE = 3u;
        const uint OPEN_EXISTING = 3u;

        var hOut = CreateFile("CONOUT$", GENERIC_READ_WRITE, FILE_SHARE_READ_WRITE,
            IntPtr.Zero, OPEN_EXISTING, 0u, IntPtr.Zero);

        try
        {
            if (hOut == INVALID_HANDLE_VALUE)
                return;

            try
            {
                var fi = new CONSOLE_FONT_INFOEX { cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>() };
                if (GetCurrentConsoleFontEx(hOut, false, ref fi))
                {
                    fi.dwFontSize.Y = (short)Math.Max(1, (int)Math.Round(fi.dwFontSize.Y * scaleFactor));
                    fi.cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>();
                    SetCurrentConsoleFontEx(hOut, false, ref fi);
                }
            }
            finally
            {
                CloseHandle(hOut);
            }
        }
        finally
        {
            FreeConsole();
        }
    }

    #endregion
}