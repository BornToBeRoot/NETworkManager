using MahApps.Metro.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace NETworkManager
{
    public partial class StatusWindow : MetroWindow, INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private MainWindow _mainWindow;

        Timer _timer = new Timer();

        private NetworkInterfaceInfo _networkInterfaceInfo;
        public NetworkInterfaceInfo NetworkInterfaceInfo
        {
            get => _networkInterfaceInfo;
            set
            {
                if (value == _networkInterfaceInfo)
                    return;

                _networkInterfaceInfo = value;
                OnPropertyChanged();
            }
        }

        private bool _isRefreshing = true;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (value == _isRefreshing)
                    return;

                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        private int _countdownValue = 10;
        public int CountdownValue
        {
            get => _countdownValue;
            set
            {
                if (value == _countdownValue)
                    return;

                _countdownValue = value;
                OnPropertyChanged();
            }
        }

        public StatusWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            DataContext = this;
            _mainWindow = mainWindow;

            _timer.Interval = 1000;
            _timer.Tick += CountdownToCloseTimer_Tick;

            // Detect if network address or status changed...
            NetworkChange.NetworkAvailabilityChanged += (sender, args) => OnNetworkHasChanged();
            NetworkChange.NetworkAddressChanged += (sender, args) => OnNetworkHasChanged();
        }

        #region ICommands & Actions
        public ICommand OpenMainWindowCommand => new RelayCommand(p => OpenMainWindowAction());

        private void OpenMainWindowAction()
        {
            HideWindow();

            if (_mainWindow.ShowWindowCommand.CanExecute(null))
                _mainWindow.ShowWindowCommand.Execute(null);
        }

        #endregion

        #region Methods
        private async void Refresh()
        {
            IsRefreshing = true;

            IPAddress detectedIP = null;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                // Try to get the ip address based on routing
                try
                {
                    detectedIP = await Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(IPAddress.Parse("1.1.1.1"));

                    break;
                }
                catch (SocketException) { }

                // If null --> check if timeout is reached
                if (detectedIP == null)
                {
                    if (stopwatch.ElapsedMilliseconds > 30000)
                        break;

                    await Task.Delay(2500);
                }
            }

            if (detectedIP == null)
            {
                IsRefreshing = false;
                // ToDo: Error Message

                return;
            }

            foreach (NetworkInterfaceInfo info in await Models.Network.NetworkInterface.GetNetworkInterfacesAsync())
            {
                if (info.IPv4Address.Contains(detectedIP))
                {
                    NetworkInterfaceInfo = info;
                    break;
                }
            }

            IsRefreshing = false;
        }

        private void OnNetworkHasChanged()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                ShowWindow();

                Refresh();

                StartCountdownToClose();
            }));
        }

        private void StartCountdownToClose()
        {
            CountdownValue = 10; // ToDo: User settings

            _timer.Start();
        }

        private void CountdownToCloseTimer_Tick(object sender, System.EventArgs e)
        {
            CountdownValue--;

            if (CountdownValue > 0)
                return;

            _timer.Stop();

            HideWindow();
        }

        public void ShowFromExternal()
        {
            ShowWindow();

            Refresh();

            StartCountdownToClose();
        }

        private void ShowWindow()
        {
            // Stop timer if running
            _timer.Stop();

            // Show on primary screen in left/bottom corner
            // ToDo: User setting...
            Left = Screen.PrimaryScreen.WorkingArea.Right - Width - 10;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height - 10;

            Show();

            // ToDo: User setting...
            Topmost = true;
        }

        private void HideWindow()
        {
            Hide();
        }
        #endregion

        private void StatusWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            HideWindow();
        }
    }
}
