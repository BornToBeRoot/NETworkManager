// Contains code from: https://stackoverflow.com/questions/5028598/hosting-external-app-in-wpf-window

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using NETworkManager.Utilities;
using NETworkManager.Models.PuTTY;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;

namespace NETworkManager.Controls
{
    public partial class PuTTYControl : INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        private bool _initialized;
        private bool _closing;       // When the tab is closed --> OnClose()

        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly PuTTYSessionInfo _puttySessionInfo;

        private Process _puttyProcess;
        private IntPtr _appWin;

        private readonly DispatcherTimer _resizeTimer = new DispatcherTimer();

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
        public PuTTYControl(PuTTYSessionInfo info)
        {
            InitializeComponent();
            DataContext = this;

            _dialogCoordinator = DialogCoordinator.Instance;

            _puttySessionInfo = info;

            _resizeTimer.Tick += ResizeTimer_Tick;
            _resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Connect after the control is drawn and only on the first init
            if (_initialized)
                return;

            // Fix: The control is not visible by default, thus height and width is not set. If the values are not set, the size does not scale properly
            PuTTYHost.Height = (int)ActualHeight;
            PuTTYHost.Width = (int)ActualWidth;

            Connect();
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
            get { return new RelayCommand(p => ReconnectAction()); }
        }

        private void ReconnectAction()
        {
            Connect();
        }
        #endregion

        #region Methods       
        private async void Connect()
        {
            IsConnecting = true;

            var info = new ProcessStartInfo
            {
                FileName = _puttySessionInfo.PuTTYLocation,
                Arguments = PuTTY.BuildCommandLine(_puttySessionInfo)
            };

            try
            {
                _puttyProcess = Process.Start(info);

                if (_puttyProcess != null)
                {
                    _puttyProcess.EnableRaisingEvents = true;
                    _puttyProcess.Exited += PuTTYProcess_Exited;

                    // Embed putty window into panel, remove border etc.
                    _puttyProcess.WaitForInputIdle();
                    _appWin = _puttyProcess.MainWindowHandle;

                    if (_appWin == IntPtr.Zero)
                    {
                        var startTime = DateTime.Now;

                        while ((DateTime.Now - startTime).TotalSeconds < 10)
                        {
                            _puttyProcess.Refresh();
                            _appWin = _puttyProcess.MainWindowHandle;

                            if (IntPtr.Zero != _appWin)
                                break;

                            await Task.Delay(50);
                        }
                    }

                    if (_appWin != IntPtr.Zero)
                    {
                        NativeMethods.SetParent(_appWin, PuTTYHost.Handle);

                        // Show window before set style and resize
                        NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.Maximize);

                        // Remove border etc.
                        long style = (int)NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
                        style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP | NativeMethods.WS_THICKFRAME);
                        NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                        IsConnected = true;

                        // Resize embedded application & refresh       
                        ResizeEmbeddedPuTTY();
                    }
                }
                else
                {
                    throw new Exception("PuTTY process could not be started!");
                }
            }
            catch (Exception ex)
            {
                if (!_closing)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = NETworkManager.Resources.Localization.Strings.OK;

                    ConfigurationManager.Current.IsDialogOpen = true;

                    await _dialogCoordinator.ShowMessageAsync(this, NETworkManager.Resources.Localization.Strings.Error,
                        ex.Message, MessageDialogStyle.Affirmative, settings);

                    ConfigurationManager.Current.IsDialogOpen = false;
                }
            }

            IsConnecting = false;
        }

        private void PuTTYProcess_Exited(object sender, EventArgs e)
        {
            // This happens when the user exit the process
            IsConnected = false;
        }

        private void ResizeEmbeddedPuTTY()
        {
            if (IsConnected)
                NativeMethods.SetWindowPos(_puttyProcess.MainWindowHandle, IntPtr.Zero, 0, 0, PuTTYHost.ClientSize.Width, PuTTYHost.ClientSize.Height, NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }

        public void Disconnect()
        {
            if (_puttyProcess != null && !_puttyProcess.HasExited)
                _puttyProcess.Kill();
        }

        public void RestartPuTTYSession()
        {
            if (IsConnected)
                NativeMethods.SendMessage(_puttyProcess.MainWindowHandle, (uint)NativeMethods.WM.SYSCOMMAND, new IntPtr(64), new IntPtr(0));
        }

        public void CloseTab()
        {
            _closing = true;

            Disconnect();
        }
        #endregion

        #region Events
        private void PuTTYGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_puttyProcess != null)
                ResizeEmbeddedPuTTY();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();

            ResizeEmbeddedPuTTY();
        }
        #endregion
    }
}