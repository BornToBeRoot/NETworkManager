using NETworkManager.Models.Network;
using NETworkManager.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class NetworkConnectionViewModel : ViewModelBase
    {
        #region  Variables 
        private bool _isChecking;
        public bool IsChecking
        {
            get => _isChecking;
            set
            {
                if (value == _isChecking)
                    return;

                _isChecking = value;
                OnPropertyChanged();
            }
        }

        #region Computer
        private bool _isComputerLoopbackIPv4Checking;
        public bool IsComputerLoopbackIPv4Checking
        {
            get => _isComputerLoopbackIPv4Checking;
            set
            {
                if (value == _isComputerLoopbackIPv4Checking)
                    return;

                _isComputerLoopbackIPv4Checking = value;
                OnPropertyChanged();
            }
        }

        private string _computerLoopbackIPv4;
        public string ComputerLoopbackIPv4
        {
            get => _computerLoopbackIPv4;
            set
            {
                if (value == _computerLoopbackIPv4)
                    return;

                _computerLoopbackIPv4 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _computerLoopbackIPv4State = ConnectionState.None;
        public ConnectionState ComputerLoopbackIPv4State
        {
            get => _computerLoopbackIPv4State;
            set
            {
                if (value == _computerLoopbackIPv4State)
                    return;

                _computerLoopbackIPv4State = value;
                OnPropertyChanged();
            }
        }

        private bool _isComputerIPv4Checking;
        public bool IsComputerIPv4Checking
        {
            get => _isComputerIPv4Checking;
            set
            {
                if (value == _isComputerIPv4Checking)
                    return;

                _isComputerIPv4Checking = value;
                OnPropertyChanged();
            }
        }

        private string _computerIPv4;
        public string ComputerIPv4
        {
            get => _computerIPv4;
            set
            {
                if (value == _computerIPv4)
                    return;

                _computerIPv4 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _computerIPv4State = ConnectionState.None;
        public ConnectionState ComputerIPv4State
        {
            get => _computerIPv4State;
            set
            {
                if (value == _computerIPv4State)
                    return;

                _computerIPv4State = value;
                OnPropertyChanged();
            }
        }

        private bool _isComputerLoopbackIPv6Checking;
        public bool IsComputerLoopbackIPv6Checking
        {
            get => _isComputerLoopbackIPv6Checking;
            set
            {
                if (value == _isComputerLoopbackIPv6Checking)
                    return;

                _isComputerLoopbackIPv6Checking = value;
                OnPropertyChanged();
            }
        }

        private string _computerLoopbackIPv6;
        public string ComputerLoopbackIPv6
        {
            get => _computerLoopbackIPv6;
            set
            {
                if (value == _computerLoopbackIPv6)
                    return;

                _computerLoopbackIPv6 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _computerLoopbackIPv6State = ConnectionState.None;
        public ConnectionState ComputerLoopbackIPv6State
        {
            get => _computerLoopbackIPv6State;
            set
            {
                if (value == _computerLoopbackIPv6State)
                    return;

                _computerLoopbackIPv6State = value;
                OnPropertyChanged();
            }
        }

        private bool _isComputerIPv6Checking;
        public bool IsComputerIPv6Checking
        {
            get => _isComputerIPv6Checking;
            set
            {
                if (value == _isComputerIPv6Checking)
                    return;

                _isComputerIPv6Checking = value;
                OnPropertyChanged();
            }
        }

        private string _computerIPv6;
        public string ComputerIPv6
        {
            get => _computerIPv6;
            set
            {
                if (value == _computerIPv6)
                    return;

                _computerIPv6 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _computerIPv6State = ConnectionState.None;
        public ConnectionState ComputerIPv6State
        {
            get => _computerIPv6State;
            set
            {
                if (value == _computerIPv6State)
                    return;

                _computerIPv6State = value;
                OnPropertyChanged();
            }
        }

        private bool _isComputerDNSChecking;
        public bool IsComputerDNSChecking
        {
            get => _isComputerDNSChecking;
            set
            {
                if (value == _isComputerDNSChecking)
                    return;

                _isComputerDNSChecking = value;
                OnPropertyChanged();
            }
        }

        private string _computerDNS;
        public string ComputerDNS
        {
            get => _computerDNS;
            set
            {
                if (value == _computerDNS)
                    return;

                _computerDNS = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _computerDNSState = ConnectionState.None;
        public ConnectionState ComputerDNSState
        {
            get => _computerDNSState;
            set
            {
                if (value == _computerDNSState)
                    return;

                _computerDNSState = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public bool CheckPublicIPAddress => SettingsManager.Current.Dashboard_CheckPublicIPAddress;
        #endregion

        #region Constructor, load settings

        public NetworkConnectionViewModel()
        {
            // Detect if network address or status changed...
            NetworkChange.NetworkAvailabilityChanged += (sender, args) => CheckConnection();
            NetworkChange.NetworkAddressChanged += (sender, args) => CheckConnection();

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            CheckConnection();
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions

        #endregion

        #region Methods
        public async Task CheckConnection()
        {
            await InitCheckConnectionAsync();
        }

        async Task InitCheckConnectionAsync()
        {
            IsChecking = true;

            List<Task> tasks = new List<Task>();

            tasks.Add(CheckConnectionComputerAsync());
            tasks.Add(CheckConnectionRouterAsync());
            tasks.Add(CheckConnectionInternetAsync());

            await Task.WhenAll(tasks);

            IsChecking = false;
        }

        public async Task CheckConnectionComputerAsync()
        {
            // Reset variables
            IsComputerLoopbackIPv4Checking = true;
            ComputerLoopbackIPv4 = "";
            ComputerLoopbackIPv4State = ConnectionState.None;
            IsComputerIPv4Checking = true;
            ComputerIPv4 = "";
            ComputerIPv4State = ConnectionState.None;
            IsComputerLoopbackIPv6Checking = true;
            ComputerLoopbackIPv6 = "";
            ComputerLoopbackIPv6State = ConnectionState.None;
            IsComputerIPv6Checking = true;
            ComputerIPv6 = "";
            ComputerIPv6State = ConnectionState.None;
            IsComputerDNSChecking = true;
            ComputerDNS = "";
            ComputerDNSState = ConnectionState.None;

            // Check tcp/ip v4 stack
            var loopbackIPv4 = "127.0.0.1";

            using (var ping = new System.Net.NetworkInformation.Ping())
            {
                for (var i = 0; i < 2; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(IPAddress.Parse(loopbackIPv4));

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        ComputerLoopbackIPv4 = loopbackIPv4;
                        ComputerLoopbackIPv4State = ConnectionState.OK;

                        break;
                    }
                    catch (PingException)
                    {
                        ComputerLoopbackIPv4 = "-/-";
                        ComputerLoopbackIPv4State = ConnectionState.Critical;
                    }
                }
            }

            IsComputerLoopbackIPv4Checking = false;

            // Detect local IPv4 address
            try
            {
                ComputerIPv4 = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicICMPTestIPAddress)).ToString();
                ComputerIPv4State = string.IsNullOrEmpty(ComputerIPv4) ? ConnectionState.Critical : ConnectionState.OK;
            }
            catch (Exception)
            {                
                ComputerIPv4 = "-/-";
                ComputerIPv4State = ConnectionState.Critical;
            }

            IsComputerIPv4Checking = false;

            // Check tcp/ip v6 stack
            var loopbackIPv6 = "::1";

            using (var ping = new System.Net.NetworkInformation.Ping())
            {
                for (var i = 0; i < 2; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(IPAddress.Parse(loopbackIPv6));

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        ComputerLoopbackIPv6 = loopbackIPv6;
                        ComputerLoopbackIPv6State = ConnectionState.OK;

                        break;
                    }
                    catch (PingException)
                    {
                        ComputerLoopbackIPv6 = "-/-";
                        ComputerLoopbackIPv6State = ConnectionState.Critical;
                    }
                }
            }

            IsComputerLoopbackIPv6Checking = false;

            // Detect local IPv6 address
            try
            {
                ComputerIPv6 = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse("2606:4700:4700::1111")).ToString();
                ComputerIPv6State = string.IsNullOrEmpty(ComputerIPv6) ? ConnectionState.Critical : ConnectionState.OK;
            }
            catch (Exception)
            {
                ComputerIPv6 = "-/-";
                ComputerIPv6State = ConnectionState.Critical;
            }

            IsComputerIPv6Checking = false;

            // Get local dns entry
            if (!string.IsNullOrEmpty(ComputerIPv4))
            {
                try
                {
                    ComputerDNS = Dns.GetHostEntry(ComputerIPv4).HostName;
                    ComputerDNSState = ConnectionState.OK;
                }
                catch (SocketException)
                {
                    ComputerDNS = "-/-";
                    ComputerDNSState = ConnectionState.Critical;
                }
            }

            IsComputerDNSChecking = false;
        }

        public async Task CheckConnectionRouterAsync()
        {
            
        }

        public async Task CheckConnectionInternetAsync()
        {

        }
        #endregion

        #region Events
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsInfo.Dashboard_CheckPublicIPAddress):
                    OnPropertyChanged(nameof(CheckPublicIPAddress));
                    break;
            }
        }
        #endregion
    }
}

