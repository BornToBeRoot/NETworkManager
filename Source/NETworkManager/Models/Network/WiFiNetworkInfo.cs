using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

namespace NETworkManager.Models.Network
{
    public class WiFiNetworkInfo : INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        #region WiFiNetwork
        public string BSSID { get; set; }
        public string SSID { get; set; }
        public int ChannelCenterFrequencyInKilohertz { get; set; }
        public byte SignalBars { get; set; }
        public bool IsWiFiDirect { get; set; }
        public double NetworkRssiInDecibelMilliwatts { get; set; }
        public WiFiPhyKind PhyKind { get; set; }
        public WiFiNetworkKind NetworkKind { get; set; }
        public NetworkAuthenticationType AuthenticationType { get; set; }
        public NetworkEncryptionType EncryptionType { get; set; }
        public TimeSpan BeaconInterval { get; set; }
        public TimeSpan Uptime { get; set; }
        #endregion

        #region View/Control
        private bool _expandDetails;
        public bool ExpandDetails
        {
            get => _expandDetails;
            set
            {      
                if (value == _expandDetails)
                    return;

                _expandDetails = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        public WiFiNetworkInfo()
        {

        }
    }
}
