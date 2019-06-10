using MahApps.Metro.Controls;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
               
        public StatusWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void MetroWindow_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Hide();
        }

        private void MetroWindow_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            RefreshNetwork();
        }

        private async void RefreshNetwork()
        {
            var detectedIP = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(System.Net.IPAddress.Parse("1.1.1.1"));

            foreach (NetworkInterfaceInfo info in await NetworkInterface.GetNetworkInterfacesAsync())
            {
                if (info.IPv4Address.Contains(detectedIP))
                    NetworkInterfaceInfo = info;
            }
        }
    }
}
