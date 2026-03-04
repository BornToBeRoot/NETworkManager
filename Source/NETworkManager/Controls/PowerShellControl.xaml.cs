// Contains code from: https://stackoverflow.com/questions/5028598/hosting-external-app-in-wpf-window

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.PowerShell;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Controls;

public partial class PowerShellControl : UserControlBase, IDragablzTabItem, IEmbeddedWindow
{
    #region Events

    private void WindowGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ResizeEmbeddedWindow();
    }

    private void WindowsFormsHost_DpiChanged(object sender, DpiChangedEventArgs e)
    {
        ResizeEmbeddedWindow();

        if (!IsConnected)
            return;

        // Rescale the console font of the embedded conhost process so it remains the same physical size when the DPI changes.
        NativeMethods.TryRescaleConsoleFont(
            (uint)_process.Id,
            e.NewDpi.PixelsPerInchX / e.OldDpi.PixelsPerInchX);
    }

    #endregion

    #region Variables

    private bool _initialized;
    private bool _closed;

    private readonly Guid _tabId;
    private readonly PowerShellSessionInfo _sessionInfo;

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

    public PowerShellControl(Guid tabId, PowerShellSessionInfo sessionInfo)
    {
        InitializeComponent();
        DataContext = this;

        ConfigurationManager.Current.PowerShellTabCount++;

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
        // Fix 3: The size needs to be scaled by the DPI, otherwise the embedded window is too small on high DPI screens (https://github.com/BornToBeRoot/NETworkManager/pull/3352)
        var dpi = System.Windows.Media.VisualTreeHelper.GetDpi(this);
        WindowHost.Height = (int)((ActualHeight - 20) * dpi.DpiScaleY);
        WindowHost.Width = (int)((ActualWidth - 20) * dpi.DpiScaleX);

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

        var info = new ProcessStartInfo
        {
            FileName = _sessionInfo.ApplicationFilePath,
            Arguments = PowerShell.BuildCommandLine(_sessionInfo)
        };

        try
        {
            _process = Process.Start(info);

            if (_process != null)
            {
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;

                // Embed window into panel, remove border etc.
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
                    // Capture DPI before embedding to correct font scaling afterwards
                    var initialWindowDpi = NativeMethods.GetDpiForWindow(_appWin);

                    NativeMethods.SetParent(_appWin, WindowHost.Handle);

                    // Show window before set style and resize
                    NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.Maximize);

                    // Remove border etc.
                    long style = (int)NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
                    style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP | NativeMethods.WS_THICKFRAME);
                    NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                    IsConnected = true;

                    // Resize after short delay — not applied immediately
                    await Task.Delay(250);

                    ResizeEmbeddedWindow();

                    // Correct font if conhost started at a different DPI than our panel
                    var currentPanelDpi = NativeMethods.GetDpiForWindow(WindowHost.Handle);

                    if (initialWindowDpi != currentPanelDpi)
                        NativeMethods.TryRescaleConsoleFont((uint)_process.Id, (double)currentPanelDpi / initialWindowDpi);
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

    public void CloseTab()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        // Disconnect the session
        Disconnect();

        ConfigurationManager.Current.PowerShellTabCount--;
    }

    #endregion
}