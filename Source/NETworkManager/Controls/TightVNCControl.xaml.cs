// Contains code from: https://stackoverflow.com/questions/5028598/hosting-external-app-in-wpf-window

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using NETworkManager.Utilities;
using NETworkManager.Models.TightVNC;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;

namespace NETworkManager.Controls
{
    public partial class TightVNCControl : INotifyPropertyChanged
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
        private bool _closing;      // When the tab is closed --> OnClose()

        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly TightVNCSessionInfo _tightVNCSessionInfo;

        private Process _tightVNCProcess;
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
        public TightVNCControl(TightVNCSessionInfo info)
        {
            InitializeComponent();
            DataContext = this;

            _dialogCoordinator = DialogCoordinator.Instance;

            _tightVNCSessionInfo = info;

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
            TightVNCHost.Height = (int)ActualHeight;
            TightVNCHost.Width = (int)ActualWidth;

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
                FileName = _tightVNCSessionInfo.TightVNCLocation,
                Arguments = TightVNC.BuildCommandLine(_tightVNCSessionInfo)
            };

            try
            {
                _tightVNCProcess = Process.Start(info);

                if (_tightVNCProcess != null)
                {
                    _tightVNCProcess.EnableRaisingEvents = true;
                    _tightVNCProcess.Exited += TightVNCProcess_Exited;

                    // Embed tightvnc window into panel, remove border etc.
                    _tightVNCProcess.WaitForInputIdle();
                    _appWin = _tightVNCProcess.MainWindowHandle;

                    if (_appWin == IntPtr.Zero)
                    {
                        var startTime = DateTime.Now;

                        while ((DateTime.Now - startTime).TotalSeconds < 10)
                        {
                            _tightVNCProcess.Refresh();
                            _appWin = _tightVNCProcess.MainWindowHandle;

                            if (IntPtr.Zero != _appWin)
                                break;

                            await Task.Delay(50);
                        }
                    }

                    if (_appWin != IntPtr.Zero)
                    {
                        NativeMethods.SetParent(_appWin, TightVNCHost.Handle);

                        // Show window before set style and resize
                        NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.Maximize);

                        // Remove border etc.
                        long style = (int)NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
                        style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP | NativeMethods.WS_THICKFRAME);
                        NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                        // Resize embedded application & refresh       
                        ResizeEmbeddedTightVNC();

                        IsConnected = true;
                    }
                }
                else
                {
                    throw new Exception("TightVNC process could not be started!");
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

        private void TightVNCProcess_Exited(object sender, EventArgs e)
        {
            // This happens when the user exit the process
            IsConnected = false;
        }

        private void ResizeEmbeddedTightVNC()
        {
            if (IsConnected)
                NativeMethods.SetWindowPos(_tightVNCProcess.MainWindowHandle, IntPtr.Zero, 0, 0, TightVNCHost.ClientSize.Width, TightVNCHost.ClientSize.Height, NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }

        public void Disconnect()
        {
            if (_tightVNCProcess != null && !_tightVNCProcess.HasExited)
                _tightVNCProcess.Kill();
        }

        public void CloseTab()
        {
            _closing = true;

            Disconnect();
        }
        #endregion

        #region Events
        private void TightVNCGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_tightVNCProcess != null)
                ResizeEmbeddedTightVNC();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();

            ResizeEmbeddedTightVNC();
        }
        #endregion
    }
}