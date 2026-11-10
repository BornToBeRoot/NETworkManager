using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Attached property that asks Windows to round the corners of a window (DWM, like the native windows), e.g. the
/// main window with its custom title bar (MahApps). Supported since Windows 11, Windows 10 ignores it (a transparent
/// window would be needed, which doesn't work with the embedded windows of Remote Desktop, PuTTY, ...). A maximized
/// window is not rounded by Windows.
/// </summary>
public static class WindowCornerHelper
{
    // https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

    // https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwm_window_corner_preference
    private const int DWMWCP_ROUND = 2;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public static readonly DependencyProperty RoundCornersProperty =
        DependencyProperty.RegisterAttached(
            "RoundCorners",
            typeof(bool),
            typeof(WindowCornerHelper),
            new PropertyMetadata(false, OnRoundCornersChanged));

    public static void SetRoundCorners(Window element, bool value) =>
        element.SetValue(RoundCornersProperty, value);

    public static bool GetRoundCorners(Window element) =>
        (bool)element.GetValue(RoundCornersProperty);

    private static void OnRoundCornersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window || !(bool)e.NewValue)
            return;

        // The handle exists after SourceInitialized
        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
            Apply(window);
        else
            window.SourceInitialized += Window_SourceInitialized;
    }

    private static void Window_SourceInitialized(object sender, EventArgs e)
    {
        if (sender is not Window window)
            return;

        window.SourceInitialized -= Window_SourceInitialized;

        Apply(window);
    }

    private static void Apply(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;

        if (handle == IntPtr.Zero)
            return;

        var preference = DWMWCP_ROUND;

        // Ignore the result: not supported (Windows 10) means square corners like before
        DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
    }
}
