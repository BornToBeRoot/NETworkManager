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

    private void WindowGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ResizeEmbeddedWindow();
    }

    private async void WindowsFormsHost_DpiChanged(object sender, DpiChangedEventArgs e)
    {
        ResizeEmbeddedWindow();

        if (!IsConnected || _process == null || _process.HasExited)
            return;

        // PuTTY is a GUI application (not console-based), so the Console Font API
        // (AttachConsole/SetCurrentConsoleFontEx) does not apply. Instead, send
        // WM_DPICHANGED directly to the PuTTY window so it can rescale its fonts
        // and layout internally — bypassing the cross-process delivery limitation.
        NativeMethods.TrySendDpiChangedMessage(
            _appWin,
            e.OldDpi.PixelsPerInchX,
            e.NewDpi.PixelsPerInchX);
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
                        // Capture PuTTY's window DPI before embedding. The process
                        // might have started on a different monitor than ours, so its font
                        // may be scaled for a different DPI. We correct this after embedding.
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
                        style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP | NativeMethods.WS_THICKFRAME);
                        NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                        IsConnected = true;

                        // Resize embedded application & refresh
                        // Requires a short delay because it's not applied immediately
                        await Task.Delay(250);

                        ResizeEmbeddedWindow();

                        // If PuTTY started at a different DPI than our panel (e.g. it
                        // spawned on a secondary monitor with a different scale factor),
                        // send WM_DPICHANGED so PuTTY rescales its fonts to match.
                        var currentPanelDpi = NativeMethods.GetDpiForWindow(WindowHost.Handle);
                        if (initialWindowDpi != currentPanelDpi)
                            NativeMethods.TrySendDpiChangedMessage(_appWin,
                                initialWindowDpi, currentPanelDpi);
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