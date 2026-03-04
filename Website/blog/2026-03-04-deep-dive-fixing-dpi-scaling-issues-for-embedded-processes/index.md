---
slug: high-dpi-embedded-processes
title: "Deep Dive: Fixing DPI Scaling Issues for Embedded Processes"
description: "When you embed a foreign process window via SetParent in WPF, it never receives DPI change notifications across process boundaries. This article shows how to rescale fonts for embedded console hosts (PowerShell) using the Console API, and for GUI processes (PuTTY) by sending WM_DPICHANGED explicitly."
authors: [borntoberoot]
tags: [wpf, dpi, windows, c#, win32, putty, powershell]
---

Modern Windows setups with multiple monitors at different scale factors (100 %, 125 %, 150 %, …) expose a hard Win32 limitation the moment you embed a foreign process window into your own application via `SetParent`. Windows simply does not forward `WM_DPICHANGED` across process boundaries.

This article documents the investigation and the two different solutions NETworkManager uses for its embedded **PowerShell** (a console host process) and **PuTTY** (a GUI process) tabs — together with the complete, relevant C# source code.

<!-- truncate -->

## The Embedding Technique

NETworkManager is a C#/WPF application that uses `WindowsFormsHost` to host a native Win32 `Panel` (WinForms `Panel`), and then calls `SetParent` to re-parent a foreign process window into that panel.

The XAML wires up the `DpiChanged` event and embeds a WinForms `Panel` as the hosting surface:

```xml
<WindowsFormsHost DpiChanged="WindowsFormsHost_DpiChanged">
    <windowsForms:Panel x:Name="WindowHost" />
</WindowsFormsHost>
```

The C# code-behind then calls `SetParent` to embed the external process window:

```csharp
// Make the external process window a child of our WinForms panel
NativeMethods.SetParent(_appWin, WindowHost.Handle);

// Strip decorations so it looks native inside our tab
long style = (int)NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP | NativeMethods.WS_THICKFRAME);
NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));
```

This works fine visually — the external window appears seamlessly inside the WPF application. **But fonts never rescale when the user drags the window to a monitor with a different DPI.**

## Why DPI Notifications Break

WPF applications declare `PerMonitorV2` DPI awareness in their manifest. When the application's `HwndSource` (the root Win32 window) moves to a different DPI monitor, Windows walks the entire Win32 window tree within the **same process** and sends `WM_DPICHANGED` / `WM_DPICHANGED_AFTERPARENT` to every child. The `WindowsFormsHost` → `WindowHost` chain is all in-process, so it receives `DpiChanged` events correctly.

The problem is that `_appWin` is owned by a **completely separate process** (PuTTY, conhost). From the Windows DWM compositor's perspective it is now a child window of your panel, but the DPI notification system only walks intra-process window trees. The external child window never receives any DPI message.

## Solution A — Console Host Processes (PowerShell, cmd)

PowerShell runs inside **conhost.exe**, the Windows console host. Unlike a GUI process, conhost exposes its font settings through the Console API (`kernel32.dll`). This is a true cross-process interface: any process can attach to an existing console and modify its font without sending any window messages.

```csharp
// NativeMethods helpers used below
[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool AttachConsole(uint dwProcessId);

[DllImport("kernel32.dll", SetLastError = true)]
public static extern bool FreeConsole();

[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
    uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
    uint dwFlagsAndAttributes, IntPtr hTemplateFile);

[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput,
    bool bMaximumWindow, ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);

[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput,
    bool bMaximumWindow, ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
```

### Font rescale helper

```csharp
/// <summary>
/// Attaches to <paramref name="processId"/>'s console and rescales its current font
/// by <paramref name="scaleFactor"/> using SetCurrentConsoleFontEx.
/// Works for any conhost-based console (PowerShell, cmd, etc.).
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
            var fi = new CONSOLE_FONT_INFOEX
            {
                cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>()
            };
            if (GetCurrentConsoleFontEx(hOut, false, ref fi))
            {
                fi.dwFontSize.Y = (short)Math.Max(1,
                    (int)Math.Round(fi.dwFontSize.Y * scaleFactor));
                fi.cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>();
                SetCurrentConsoleFontEx(hOut, false, ref fi);
            }
        }
        finally { CloseHandle(hOut); }
    }
    finally { FreeConsole(); }
}
```

### Using it in the PowerShell control

`WindowsFormsHost` raises `DpiChanged` when the parent WPF window moves monitors. The handler uses the `NewDpi / OldDpi` ratio to rescale the font relatively:

```csharp
private void WindowsFormsHost_DpiChanged(object sender, DpiChangedEventArgs e)
{
    ResizeEmbeddedWindow();

    if (!IsConnected)
        return;

    // Rescale the console font using the new/old DPI ratio via the Console API.
    // WM_DPICHANGED is never forwarded to cross-process child windows,
    // so we use AttachConsole + SetCurrentConsoleFontEx instead.
    NativeMethods.TryRescaleConsoleFont(
        (uint)_process.Id,
        e.NewDpi.PixelsPerInchX / e.OldDpi.PixelsPerInchX);
}
```

### Fixing the initial DPI baseline

There is a subtle bug if the embedded process spawns on a **different monitor** than NETworkManager: conhost's font is scaled for *its* monitor's DPI, not ours. Every subsequent `newDpi/oldDpi` relative rescale will then compound the error.

Fix: read `GetDpiForWindow` for both windows **before** `SetParent`, and correct the baseline immediately after embedding:

```csharp
// Capture DPI before embedding to correct font scaling afterwards
var initialWindowDpi = NativeMethods.GetDpiForWindow(_appWin);

NativeMethods.SetParent(_appWin, WindowHost.Handle);
// ... ShowWindow, strip styles, IsConnected = true ...

await Task.Delay(250);
ResizeEmbeddedWindow();

// Correct font if conhost started at a different DPI than our panel
var currentPanelDpi = NativeMethods.GetDpiForWindow(WindowHost.Handle);
if (initialWindowDpi != currentPanelDpi)
    NativeMethods.TryRescaleConsoleFont(
        (uint)_process.Id,
        (double)currentPanelDpi / initialWindowDpi);
```

`GetDpiForWindow` is available since Windows 10 version 1607:

```csharp
[DllImport("user32.dll")]
public static extern uint GetDpiForWindow(IntPtr hWnd);
```

## Solution B — GUI Processes (PuTTY)

PuTTY is a standard Win32 GUI application, not a console. The Console API does not apply. Instead, the approach is to send `WM_DPICHANGED` (0x02E0) directly to the PuTTY window, which it handles natively (requires PuTTY 0.75+ for reliable behaviour).

`WM_DPICHANGED` carries the new DPI packed into `wParam` (LOWORD = DPI X, HIWORD = DPI Y) and a `RECT*` in `lParam` with the suggested new window rect:

### DPI message helper

```csharp
/// <summary>
/// Sends WM_DPICHANGED to a GUI window so it can rescale its fonts and layout.
/// WM_DPICHANGED is not reliably forwarded to cross-process child windows
/// embedded via SetParent, so we send it explicitly.
/// </summary>
public static void TrySendDpiChangedMessage(IntPtr hWnd, double oldDpi, double newDpi)
{
    if (hWnd == IntPtr.Zero)
        return;

    if (Math.Abs(newDpi - oldDpi) < 0.01)
        return;

    const uint WM_DPICHANGED = 0x02E0;

    var newDpiInt = (int)Math.Round(newDpi);
    // HIWORD = Y DPI, LOWORD = X DPI
    var wParam = (IntPtr)((newDpiInt << 16) | newDpiInt);

    // lParam must point to a RECT with the suggested new size/position.
    var rect = new RECT();
    GetWindowRect(hWnd, ref rect);

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
```

### Using it in the PuTTY control

```csharp
private void WindowsFormsHost_DpiChanged(object sender, DpiChangedEventArgs e)
{
    ResizeEmbeddedWindow();

    if (!IsConnected)
        return;

    // Send WM_DPICHANGED explicitly to the PuTTY window with the new DPI.
    // WM_DPICHANGED is never forwarded to cross-process child windows after SetParent,
    // so we inject the message directly.
    NativeMethods.TrySendDpiChangedMessage(
        _appWin,
        e.OldDpi.PixelsPerInchX,
        e.NewDpi.PixelsPerInchX);
}
```

### Fixing the initial DPI baseline

Same issue as PowerShell: PuTTY may start on a different monitor. Because PuTTY is a GUI process, the console API does not apply — but the explicit `WM_DPICHANGED` message works for the initial correction too:

```csharp
// Capture DPI before embedding
var initialWindowDpi = NativeMethods.GetDpiForWindow(_appWin);

NativeMethods.SetParent(_appWin, WindowHost.Handle);
// ... ShowWindow, strip styles, IsConnected = true ...

await Task.Delay(250);
ResizeEmbeddedWindow();

// Correct DPI if PuTTY started at a different DPI than our panel
var currentPanelDpi = NativeMethods.GetDpiForWindow(WindowHost.Handle);
if (initialWindowDpi != currentPanelDpi)
    NativeMethods.TrySendDpiChangedMessage(_appWin, initialWindowDpi, currentPanelDpi);
```

## Sizing the WindowsFormsHost at Load

One more pitfall: `WindowsFormsHost` starts with zero size because WPF's logical pixel coordinates do not account for the system DPI. The panel's `ClientSize` must be set in **physical pixels**:

```csharp
private void UserControl_Loaded(object sender, RoutedEventArgs e)
{
    if (_initialized) return;

    // VisualTreeHelper.GetDpi returns DpiScaleX/Y as a ratio (1.0 = 96 DPI, 1.5 = 144 DPI).
    var dpi = System.Windows.Media.VisualTreeHelper.GetDpi(this);
    WindowHost.Height = (int)(ActualHeight * dpi.DpiScaleY);
    WindowHost.Width  = (int)(ActualWidth  * dpi.DpiScaleX);

    Connect().ConfigureAwait(false);
    _initialized = true;
}
```

## Summary

When you embed a foreign process window via `SetParent`, Windows never forwards DPI change notifications across process boundaries. For console host processes (PowerShell, cmd) use the Windows Console API (`AttachConsole` + `SetCurrentConsoleFontEx`) to rescale fonts directly; for GUI processes (PuTTY) send `WM_DPICHANGED` (0x02E0) explicitly with the new DPI packed into `wParam`. In both cases, apply an initial DPI correction after `SetParent` by comparing `GetDpiForWindow` before and after embedding, and set the `WindowsFormsHost` initial size in physical pixels using `VisualTreeHelper.GetDpi`.

The full implementation is available in the NETworkManager source:

- [`NETworkManager.Utilities/NativeMethods.cs`](https://github.com/BornToBeRoot/NETworkManager/blob/main/Source/NETworkManager.Utilities/NativeMethods.cs) — all P/Invoke declarations and helpers
- [`NETworkManager/Controls/PowerShellControl.xaml.cs`](https://github.com/BornToBeRoot/NETworkManager/blob/main/Source/NETworkManager/Controls/PowerShellControl.xaml.cs) — console host approach
- [`NETworkManager/Controls/PuTTYControl.xaml.cs`](https://github.com/BornToBeRoot/NETworkManager/blob/main/Source/NETworkManager/Controls/PuTTYControl.xaml.cs) — GUI process approach
