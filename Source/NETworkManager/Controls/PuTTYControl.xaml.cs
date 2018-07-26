// Contains code from: https://stackoverflow.com/questions/5028598/hosting-external-app-in-wpf-window

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;
using System.Windows.Threading;
using System.Diagnostics;
using NETworkManager.Utilities;
using NETworkManager.Models.PuTTY;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;

namespace NETworkManager.Controls
{
    public partial class PuttyControl : INotifyPropertyChanged
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
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly PuTTYProfileInfo _puttyProfileInfo;

        Process _puttyProcess;
        private IntPtr _appWin;

        private readonly DispatcherTimer _resizeTimer = new DispatcherTimer();

        private bool _connected = true;
        public bool Connected
        {
            get => _connected;
            set
            {
                if (value == _connected)
                    return;

                _connected = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load
        public PuttyControl(PuTTYProfileInfo info)
        {
            InitializeComponent();
            DataContext = this;

            _dialogCoordinator = DialogCoordinator.Instance;

            _puttyProfileInfo = info;

            _resizeTimer.Tick += ResizeTimer_Tick;
            _resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Connect after the control is drawn and only on the first init
            if (_initialized)
                return;

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
            var info = new ProcessStartInfo
            {
                FileName = _puttyProfileInfo.PuTTYLocation,
                Arguments = PuTTY.BuildCommandLine(_puttyProfileInfo)
            };

            try
            {
                _puttyProcess = Process.Start(info);

                if (_puttyProcess != null)
                {
                    _puttyProcess.EnableRaisingEvents = true;
                    _puttyProcess.Exited += PuTTYProcess_Exited;

                    _puttyProcess.WaitForInputIdle();

                    // Embed putty window into panel, remove border etc.
                    _appWin = _puttyProcess.MainWindowHandle;

                    NativeMethods.SetParent(_appWin, PuttyHost.Handle);

                    // Show window before set style and resize
                    NativeMethods.ShowWindow(_appWin, NativeMethods.WindowShowStyle.Maximize);

                    // Remove border etc.
                    long style = (int)NativeMethods.GetWindowLong(_appWin, NativeMethods.GWL_STYLE);
                    style &= ~(NativeMethods.WS_CAPTION | NativeMethods.WS_POPUP | NativeMethods.WS_THICKFRAME);
                    NativeMethods.SetWindowLongPtr(_appWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                    // Resize embedded application & refresh       
                    ResizeEmbeddedPutty();

                    Connected = true;
                }
                else
                {
                    throw new Exception("PuTTY process could not be started!");
                }
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

                ConfigurationManager.Current.IsDialogOpen = true;

                await _dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Error"), ex.Message, MessageDialogStyle.Affirmative, settings);

                ConfigurationManager.Current.IsDialogOpen = false;
            }
        }

        private void PuTTYProcess_Exited(object sender, EventArgs e)
        {
            // This happens when the user exit the process
            Connected = false;
        }

        private void ResizeEmbeddedPutty()
        {
            NativeMethods.SetWindowPos(_puttyProcess.MainWindowHandle, IntPtr.Zero, 0, 0, PuttyHost.ClientSize.Width, PuttyHost.ClientSize.Height, NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }

        public void Disconnect()
        {
            if (_puttyProcess != null && !_puttyProcess.HasExited)
                _puttyProcess.Kill();
        }

        public void CloseTab()
        {
            Disconnect();
        }
        #endregion

        #region Events
        private void PuTTYGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_puttyProcess != null)
                ResizeEmbeddedPutty();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();

            ResizeEmbeddedPutty();
        }
        #endregion
    }
}