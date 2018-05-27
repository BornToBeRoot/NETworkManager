// Contains code from: https://stackoverflow.com/questions/5028598/hosting-external-app-in-wpf-window

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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
    public partial class PuTTYControl : UserControl, INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        private bool _initialized = false;
        private IDialogCoordinator dialogCoordinator;

        private Models.PuTTY.PuTTYSessionInfo _puTTYSessionInfo;

        Process PuTTYProcess = null;
        IntPtr AppWin;

        DispatcherTimer resizeTimer = new DispatcherTimer();

        private bool _connected = true;
        public bool Connected
        {
            get { return _connected; }
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
        public PuTTYControl(Models.PuTTY.PuTTYSessionInfo info)
        {
            InitializeComponent();
            DataContext = this;

            dialogCoordinator = DialogCoordinator.Instance;

            _puTTYSessionInfo = info;

            resizeTimer.Tick += ResizeTimer_Tick;
            resizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Connect after the control is drawn and only on the first init
            if (!_initialized)
            {
                Connect();
                _initialized = true;
            }
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


            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = _puTTYSessionInfo.PuTTYLocation,
                Arguments = PuTTY.BuildCommandLine(_puTTYSessionInfo)
            };

            try
            {
                PuTTYProcess = Process.Start(info);

                PuTTYProcess.EnableRaisingEvents = true;
                PuTTYProcess.Exited += PuTTYProcess_Exited;

                PuTTYProcess.WaitForInputIdle();

                // Embed putty window into panel, remove border etc.
                AppWin = PuTTYProcess.MainWindowHandle;

                NativeMethods.SetParent(AppWin, puTTYHost.Handle);

                // Show window before set style and resize
                NativeMethods.ShowWindow(AppWin, NativeMethods.WindowShowStyle.Maximize);

                // Remove border etc.
                int style = (int)NativeMethods.GetWindowLong(AppWin, NativeMethods.GWL_STYLE);
                style &= ~(NativeMethods.WS_BORDER | NativeMethods.WS_THICKFRAME);
                NativeMethods.SetWindowLongPtr(AppWin, NativeMethods.GWL_STYLE, new IntPtr(style));

                // Resize embedded application & refresh       
                if (PuTTYProcess != null)
                    ResizeEmbeddedPuTTY();

                Connected = true;
            }
            catch (Exception ex)
            {
                MetroDialogSettings settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = LocalizationManager.GetStringByKey("String_Button_OK");

                ConfigurationManager.Current.FixAirspace = true;

                await dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_Error"), ex.Message, MessageDialogStyle.Affirmative, settings);

                ConfigurationManager.Current.FixAirspace = false;
            }
        }

        private void PuTTYProcess_Exited(object sender, EventArgs e)
        {
            // This happens when the user exit the process
            Connected = false;
        }

        private void ResizeEmbeddedPuTTY()
        {
            NativeMethods.SetWindowPos(PuTTYProcess.MainWindowHandle, IntPtr.Zero, 0, 0, puTTYHost.ClientSize.Width, puTTYHost.ClientSize.Height, NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }

        public void Disconnect()
        {
            if (PuTTYProcess != null && !PuTTYProcess.HasExited)
                PuTTYProcess.Kill();
        }

        public void CloseTab()
        {
            Disconnect();
        }
        #endregion

        #region Events
        private void PuTTYGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (PuTTYProcess != null)
                ResizeEmbeddedPuTTY();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop();

            ResizeEmbeddedPuTTY();
        }
        #endregion
    }
}