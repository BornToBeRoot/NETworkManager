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
using NETworkManager.Settings;
using PuTTY = NETworkManager.Models.PuTTY.PuTTY;

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

        private readonly PuTTYSessionInfo _sessionInfo;

        private Process _process;
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

            _sessionInfo = info;

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
            WindowHost.Height = (int)ActualHeight;
            WindowHost.Width = (int)ActualWidth;

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
            Reconnect();
        }
        #endregion

        #region Methods       
        private async Task Connect()
        {
            IsConnecting = true;

            // Create log path
            DirectoryCreator.CreateWithEnvironmentVariables(_sessionInfo.LogPath);

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
                            // Fix for netcore3.1 https://stackoverflow.com/questions/60342879/process-mainwindowhandle-is-non-zero-in-net-framework-but-zero-in-net-core-unl
                            /*
                            try
                            {
                                _process = Process.GetProcessById(_process.Id);
                            }
                            catch
                            {
                                break; // Process has exited
                            }
                            */


                            _process.Refresh();
                            
                            if (_process.HasExited)
                                break;
                            

                            _appWin = _process.MainWindowHandle;

                            if (IntPtr.Zero != _appWin)
                                break;

                            await Task.Delay(50);
                        }
                    }

                    if (_appWin != IntPtr.Zero)
                    {
                        while (!_process.HasExited && _process.MainWindowTitle.IndexOf(" - PuTTY", StringComparison.Ordinal) == -1)
                        {
                            await Task.Delay(50);

                            _process.Refresh();
                        }

                        if (!_process.HasExited)
                        {
                            NativeMethods.SetParent(_appWin, WindowHost.Handle);

                            // Show window before set style and resize
                            NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.Maximize);

                            // Remove border etc.
                            long style = (int)NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
                            style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP | NativeMethods.WS_THICKFRAME); // NativeMethods.WS_POPUP --> Overflow? (https://github.com/BornToBeRoot/NETworkManager/issues/167)
                            NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                            IsConnected = true;

                            // Resize embedded application & refresh       
                            ResizeEmbeddedWindow();
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
                if (!_closing)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    ConfigurationManager.Current.FixAirspace = true;

                    await _dialogCoordinator.ShowMessageAsync(this, NETworkManager.Localization.Resources.Strings.Error,
                        ex.Message, MessageDialogStyle.Affirmative, settings);

                    ConfigurationManager.Current.FixAirspace = false;
                }
            }

            IsConnecting = false;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            // This happens when the user exit the process
            IsConnected = false;
        }

        private void ResizeEmbeddedWindow()
        {
            if (IsConnected)
                NativeMethods.SetWindowPos(_process.MainWindowHandle, IntPtr.Zero, 0, 0, WindowHost.ClientSize.Width, WindowHost.ClientSize.Height, NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }

        public void Disconnect()
        {
            if (_process != null && !_process.HasExited)
                _process.Kill();
        }

        private void Reconnect()
        {
            if (IsConnected)
                Disconnect();

            Connect();
        }

        public void RestartSession()
        {
            if (IsConnected)
                NativeMethods.SendMessage(_process.MainWindowHandle, (uint)NativeMethods.WM.SYSCOMMAND, new IntPtr(64), new IntPtr(0));
        }

        public void CloseTab()
        {
            _closing = true;

            Disconnect();
        }
        #endregion

        #region Events
        private void WindowGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_process != null)
                ResizeEmbeddedWindow();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();

            ResizeEmbeddedWindow();
        }
        #endregion
    }
}