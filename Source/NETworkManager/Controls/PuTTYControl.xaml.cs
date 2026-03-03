// Contains code from: https://stackoverflow.com/questions/5028598/hosting-external-app-in-wpf-window

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.PuTTY;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using PuTTY = NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Controls;

public partial class PuTTYControl : UserControlBase, IDragablzTabItem, IEmbeddedWindow
{
    #region Events

    private bool _isDpiChanging;

    private void WindowGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ResizeEmbeddedWindow();
    }

    private async void WindowsFormsHost_DpiChanged(object sender, DpiChangedEventArgs e)
    {
        if (!IsConnected || _appWin == IntPtr.Zero || _isDpiChanging)
            return;

        await TriggerDpiUpdateAsync();
    }

    /// <summary>
    /// Detaches PuTTY's window, places it visibly on the current monitor so Windows
    /// delivers WM_DPICHANGED natively to PuTTY's message loop, then re-embeds it.
    /// Called both on initial connect (if DPIs differ) and on every DPI change.
    /// </summary>
    private async Task TriggerDpiUpdateAsync()
    {
        if (_isDpiChanging || !IsConnected || _appWin == IntPtr.Zero)
            return;

        _isDpiChanging = true;

        try
        {
            // Use MonitorFromWindow for reliable physical-pixel coordinates — WinForms's
            // Screen.FromHandle can return DPI-virtualized (wrong) coordinates on
            // PerMonitorV2 setups.
            var bounds = NativeMethods.GetMonitorBoundsForWindow(WindowHost.Handle);
            int cx = bounds.left + (bounds.right - bounds.left) / 2;
            int cy = bounds.top + (bounds.bottom - bounds.top) / 2;

            // Temporarily restore WS_POPUP so that after SetParent(null) Windows treats
            // the window as a proper top-level popup. Without WS_POPUP (stripped during
            // embedding), the window has no recognised top-level style and Windows
            // skips sending WM_DPICHANGED to it entirely.
            // Also clear WS_CHILD if SetParent happened to set it.
            long style = NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
            style &= ~NativeMethods.WS_CHILD;
            style |= NativeMethods.WS_POPUP;
            NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

            NativeMethods.SetParent(_appWin, IntPtr.Zero);

            // Place as an 800×600 window at the centre of the target monitor, behind
            // all other windows (HWND_BOTTOM). SWP_SHOWWINDOW makes it visible so
            // Windows detects the monitor's DPI and delivers WM_DPICHANGED natively.
            // A 1×1 window is below the threshold Windows uses for DPI detection.
            NativeMethods.SetWindowPos(_appWin, NativeMethods.HWND_BOTTOM, cx, cy, 800, 600,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);

            // Give PuTTY's message loop time to dequeue and handle WM_DPICHANGED.
            await Task.Delay(300);

            if (!IsConnected || _appWin == IntPtr.Zero)
                return;

            NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.Hide);
            NativeMethods.SetParent(_appWin, WindowHost.Handle);

            // Remove WS_POPUP after re-embedding; keeping it on a child window can
            // cause painting to overflow the parent panel (see issue #167).
            style = NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
            style &= ~NativeMethods.WS_POPUP;
            NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

            ResizeEmbeddedWindow();
            NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.ShowNoActivate);
        }
        finally
        {
            _isDpiChanging = false;
        }
    }

    #endregion

    #region Variables

    private bool _initialized;
    private bool _closed;

    private readonly Guid _tabId;
    private readonly PuTTYSessionInfo _sessionInfo;

    private Process _process;
    private IntPtr _appWin;

    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (value == _isConnected)
                return;

            _isConnected = value;
            OnPropertyChanged();
        }
    }

    private bool _isConnecting;

    public bool IsConnecting
    {
        get => _isConnecting;
        set
        {
            if (value == _isConnecting)
                return;

            _isConnecting = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load

    public PuTTYControl(Guid tabId, PuTTYSessionInfo sessionInfo)
    {
        InitializeComponent();
        DataContext = this;

        ConfigurationManager.Current.PuTTYTabCount++;

        _tabId = tabId;
        _sessionInfo = sessionInfo;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect after the control is drawn and only on the first init
        if (_initialized)
            return;

        // Fix 1: The control is not visible by default, thus height and width is not set. If the values are not set, the size does not scale properly
        // Fix 2: Somehow the initial size need to be 20px smaller than the actual size after using Dragablz (https://github.com/BornToBeRoot/NETworkManager/pull/2678)
        WindowHost.Height = (int)ActualHeight - 20;
        WindowHost.Width = (int)ActualWidth - 20;

        Connect().ConfigureAwait(false);

        _initialized = true;
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        CloseTab();
    }

    #endregion

    #region ICommands & Actions

    public ICommand ReconnectCommand
    {
        get { return new RelayCommand(_ => ReconnectAction()); }
    }

    private void ReconnectAction()
    {
        Reconnect();
    }

    #endregion

    #region Methods

    private async Task Connect()
    {
        IsConnecting = true;

        // Create log path
        DirectoryHelper.CreateWithEnvironmentVariables(_sessionInfo.LogPath);

        var info = new ProcessStartInfo
        {
            FileName = _sessionInfo.ApplicationFilePath,
            Arguments = PuTTY.BuildCommandLine(_sessionInfo)
        };

        try
        {
            _process = Process.Start(info);

            if (_process != null)
            {
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;

                _appWin = _process.MainWindowHandle;

                if (_appWin == IntPtr.Zero)
                {
                    var startTime = DateTime.Now;

                    while ((DateTime.Now - startTime).TotalSeconds < 10)
                    {
                        _process.Refresh();

                        if (_process.HasExited)
                            break;


                        _appWin = _process.MainWindowHandle;

                        if (IntPtr.Zero != _appWin)
                            break;

                        await Task.Delay(100);
                    }
                }

                if (_appWin != IntPtr.Zero)
                {
                    while (!_process.HasExited &&
                           _process.MainWindowTitle.IndexOf(" - PuTTY", StringComparison.Ordinal) == -1)
                    {
                        await Task.Delay(100);

                        _process.Refresh();
                    }

                    if (!_process.HasExited)
                    {
                        // Capture PuTTY's DPI before embedding. If it started on a
                        // different monitor than ours, we correct below via detach/reattach.
                        var initialWindowDpi = NativeMethods.GetDpiForWindow(_appWin);

                        // Enable mixed-DPI hosting on this thread before SetParent so that
                        // Windows routes DPI notifications to the cross-process child window.
                        // SetThreadDpiHostingBehavior is available on Windows 10 1803+.
                        var prevDpiHosting = NativeMethods.SetThreadDpiHostingBehavior(
                            NativeMethods.DPI_HOSTING_BEHAVIOR.DPI_HOSTING_BEHAVIOR_MIXED);
                        NativeMethods.SetParent(_appWin, WindowHost.Handle);
                        NativeMethods.SetThreadDpiHostingBehavior(prevDpiHosting);

                        // Show window before set style and resize
                        NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.Maximize);

                        // Remove border etc.
                        long style = (int)NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
                        style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP |
                                   NativeMethods
                                       .WS_THICKFRAME); // NativeMethods.WS_POPUP --> Overflow? (https://github.com/BornToBeRoot/NETworkManager/issues/167)
                        NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                        IsConnected = true;

                        // Resize embedded application & refresh
                        // Requires a short delay because it's not applied immediately
                        await Task.Delay(250);

                        ResizeEmbeddedWindow();

                        // If PuTTY started on a different DPI monitor than ours, trigger
                        // a DPI correction now so its font matches the current monitor.
                        var currentPanelDpi = NativeMethods.GetDpiForWindow(WindowHost.Handle);
                        if (initialWindowDpi != currentPanelDpi)
                            await TriggerDpiUpdateAsync();
                    }
                }
            }
            else
            {
                throw new Exception("Process could not be started!");
            }
        }
        catch (Exception ex)
        {
            if (!_closed)
                // Use built-in message box because we have visual issues in the dragablz window
                MessageBox.Show(ex.Message, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        IsConnecting = false;
    }

    private void Process_Exited(object sender, EventArgs e)
    {
        // This happens when the user exit the process
        IsConnected = false;
    }

    public void FocusEmbeddedWindow()
    {
        if (IsConnected)
            NativeMethods.SetForegroundWindow(_process.MainWindowHandle);
    }

    public void ResizeEmbeddedWindow()
    {
        if (IsConnected)
            NativeMethods.SetWindowPos(_process.MainWindowHandle, IntPtr.Zero, 0, 0, WindowHost.ClientSize.Width,
                WindowHost.ClientSize.Height, NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
    }

    private void Disconnect()
    {
        if (IsConnected)
            _process.Kill();
    }

    private void Reconnect()
    {
        if (IsConnected)
            Disconnect();

        Connect().ConfigureAwait(false);
    }

    public void RestartSession()
    {
        if (IsConnected)
            NativeMethods.SendMessage(_process.MainWindowHandle, (uint)NativeMethods.WM.SYSCOMMAND, new IntPtr(64),
                new IntPtr(0));
    }

    public void CloseTab()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        // Disconnect the session
        Disconnect();

        ConfigurationManager.Current.PuTTYTabCount--;
    }

    #endregion
}