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
    public const long WS_POPUP = 0x80000000L;
    public const long WS_CAPTION = 0x00C00000L;

    /// <summary>The value returned by CreateFile on failure.</summary>
    public static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

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

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
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

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// Returns the DPI (dots per inch) value for the monitor that contains the specified window.
    /// Returns 0 if the window handle is invalid. Available on Windows 10 version 1607+.
    /// </summary>
    [DllImport("user32.dll")]
    public static extern uint GetDpiForWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

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

    /// <summary>
    /// Sends a <c>WM_DPICHANGED</c> message to a GUI window (e.g. PuTTY) so it can
    /// rescale its fonts and layout internally. This is necessary because
    /// <c>WM_DPICHANGED</c> is not reliably forwarded to cross-process child windows
    /// embedded via <c>SetParent</c>. Requires PuTTY 0.75+ to take effect.
    /// </summary>
    public static void TrySendDpiChangedMessage(IntPtr hWnd, double oldDpi, double newDpi)
    {
        if (hWnd == IntPtr.Zero)
            return;

        if (Math.Abs(newDpi - oldDpi) < 0.01)
            return;

        const uint WM_DPICHANGED = 0x02E0;

        var newDpiInt = (int)Math.Round(newDpi);
        var wParam = (IntPtr)((newDpiInt << 16) | newDpiInt); // HIWORD = Y DPI, LOWORD = X DPI

        // Build the suggested new rect from the current window position.
        var rect = new RECT();
        GetWindowRect(hWnd, ref rect);

        // lParam must point to a RECT with the suggested new size/position.
        var lParam = Marshal.AllocHGlobal(Marshal.SizeOf<RECT>());
        try
        {
            Marshal.StructureToPtr(rect, lParam, false);
            SendMessage(hWnd, WM_DPICHANGED, wParam, lParam);
        }
        finally
        {
            Marshal.FreeHGlobal(lParam);
        }
    }

    #endregion
}