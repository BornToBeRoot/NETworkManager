using MahApps.Metro.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        public StatusWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            DataContext = this;
            _mainWindow = mainWindow;

            UpdateNetworkInterfaceInfo();
        }

        #region ICommands & Actions
        public ICommand OpenMainWindowCommand => new RelayCommand(p => OpenMainWindowAction());

        private void OpenMainWindowAction()
        {
            Hide();

            if (_mainWindow.ShowWindowCommand.CanExecute(null))
                _mainWindow.ShowWindowCommand.Execute(null);
        }

        #endregion

        #region Methods

        public void Refresh()
        {
            UpdateNetworkInterfaceInfo();
        }

        private async void UpdateNetworkInterfaceInfo()
        {
            IsRefreshing = true;

            IPAddress detectedIP = null;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while(true)
            {
                // Try to get the ip address based on routing
                try
                {
                    detectedIP = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(IPAddress.Parse("1.1.1.1"));

                    break;
                }
                catch(SocketException) {}
                
                // If null --> check if timeout is reached
                if(detectedIP == null)
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

            foreach (NetworkInterfaceInfo info in await NetworkInterface.GetNetworkInterfacesAsync())
            {
                if (info.IPv4Address.Contains(detectedIP))
                {
                    NetworkInterfaceInfo = info;
                    break;
                }
            }

            IsRefreshing = false;
        }
        #endregion

        private void StatusWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }
    }
}
