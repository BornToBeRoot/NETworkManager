// Contains code from: https://stackoverflow.com/questions/5028598/hosting-external-app-in-wpf-window

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.AWS;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Controls;

public partial class AWSSessionManagerControl : UserControlBase, IDragablzTabItem, IEmbeddedWindow
{
    #region Events

    private void WindowGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ResizeEmbeddedWindow();
    }

    #endregion

    #region Variables

    private bool _initialized;
    private bool _closed;

    private readonly IDialogCoordinator _dialogCoordinator;

    private readonly Guid _tabId;
    private readonly AWSSessionManagerSessionInfo _sessionInfo;

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

    public AWSSessionManagerControl(Guid tabId, AWSSessionManagerSessionInfo sessionInfo)
    {
        InitializeComponent();
        DataContext = this;

        _dialogCoordinator = DialogCoordinator.Instance;

        ConfigurationManager.Current.AWSSessionManagerTabCount++;

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

        var info = new ProcessStartInfo
        {
            FileName = _sessionInfo.ApplicationFilePath,
            Arguments = AWSSessionManager.BuildCommandLine(_sessionInfo)
        };

        try
        {
            _process = Process.Start(info);

            if (_process != null)
            {
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;

                // Embed window into panel, remove border etc.
                //  _process.WaitForInputIdle();
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
                    NativeMethods.SetParent(_appWin, WindowHost.Handle);

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
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                ConfigurationManager.OnDialogOpen();

                await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                    ex.Message, MessageDialogStyle.Affirmative, settings);

                ConfigurationManager.OnDialogClose();
            }
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

        ConfigurationManager.Current.AWSSessionManagerTabCount--;
    }

    #endregion
}