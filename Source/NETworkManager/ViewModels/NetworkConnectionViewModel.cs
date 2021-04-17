using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
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

        #region Router
        private bool _isRouterIPv4Checking;
        public bool IsRouterIPv4Checking
        {
            get => _isRouterIPv4Checking;
            set
            {
                if (value == _isRouterIPv4Checking)
                    return;

                _isRouterIPv4Checking = value;
                OnPropertyChanged();
            }
        }

        private string _routerIPv4;
        public string RouterIPv4
        {
            get => _routerIPv4;
            set
            {
                if (value == _routerIPv4)
                    return;

                _routerIPv4 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _routerIPv4State = ConnectionState.None;
        public ConnectionState RouterIPv4State
        {
            get => _routerIPv4State;
            set
            {
                if (value == _routerIPv4State)
                    return;

                _routerIPv4State = value;
                OnPropertyChanged();
            }
        }

        private bool _isRouterIPv6Checking;
        public bool IsRouterIPv6Checking
        {
            get => _isRouterIPv6Checking;
            set
            {
                if (value == _isRouterIPv6Checking)
                    return;

                _isRouterIPv6Checking = value;
                OnPropertyChanged();
            }
        }

        private string _routerIPv6;
        public string RouterIPv6
        {
            get => _routerIPv6;
            set
            {
                if (value == _routerIPv6)
                    return;

                _routerIPv6 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _routerIPv6State = ConnectionState.None;
        public ConnectionState RouterIPv6State
        {
            get => _routerIPv6State;
            set
            {
                if (value == _routerIPv6State)
                    return;

                _routerIPv6State = value;
                OnPropertyChanged();
            }
        }

        private bool _isRouterDNSChecking;
        public bool IsRouterDNSChecking
        {
            get => _isRouterDNSChecking;
            set
            {
                if (value == _isRouterDNSChecking)
                    return;

                _isRouterDNSChecking = value;
                OnPropertyChanged();
            }
        }

        private string _routerDNS;
        public string RouterDNS
        {
            get => _routerDNS;
            set
            {
                if (value == _routerDNS)
                    return;

                _routerDNS = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _routerDNSState = ConnectionState.None;
        public ConnectionState RouterDNSState
        {
            get => _routerDNSState;
            set
            {
                if (value == _routerDNSState)
                    return;

                _routerDNSState = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Internet
        private bool _isInternetIPv4Checking;
        public bool IsInternetIPv4Checking
        {
            get => _isInternetIPv4Checking;
            set
            {
                if (value == _isInternetIPv4Checking)
                    return;

                _isInternetIPv4Checking = value;
                OnPropertyChanged();
            }
        }

        private string _internetIPv4;
        public string InternetIPv4
        {
            get => _internetIPv4;
            set
            {
                if (value == _internetIPv4)
                    return;

                _internetIPv4 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _internetIPv4State = ConnectionState.None;
        public ConnectionState InternetIPv4State
        {
            get => _internetIPv4State;
            set
            {
                if (value == _internetIPv4State)
                    return;

                _internetIPv4State = value;
                OnPropertyChanged();
            }
        }

        private bool _isInternetIPv6Checking;
        public bool IsInternetIPv6Checking
        {
            get => _isInternetIPv6Checking;
            set
            {
                if (value == _isInternetIPv6Checking)
                    return;

                _isInternetIPv6Checking = value;
                OnPropertyChanged();
            }
        }

        private string _internetIPv6;
        public string InternetIPv6
        {
            get => _internetIPv6;
            set
            {
                if (value == _internetIPv6)
                    return;

                _internetIPv6 = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _internetIPv6State = ConnectionState.None;
        public ConnectionState InternetIPv6State
        {
            get => _internetIPv6State;
            set
            {
                if (value == _internetIPv6State)
                    return;

                _internetIPv6State = value;
                OnPropertyChanged();
            }
        }

        private bool _isInternetDNSChecking;
        public bool IsInternetDNSChecking
        {
            get => _isInternetDNSChecking;
            set
            {
                if (value == _isInternetDNSChecking)
                    return;

                _isInternetDNSChecking = value;
                OnPropertyChanged();
            }
        }

        private string _internetDNS;
        public string InternetDNS
        {
            get => _internetDNS;
            set
            {
                if (value == _internetDNS)
                    return;

                _internetDNS = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _internetDNSState = ConnectionState.None;
        public ConnectionState InternetDNSState
        {
            get => _internetDNSState;
            set
            {
                if (value == _internetDNSState)
                    return;

                _internetDNSState = value;
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
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions

        #endregion

        #region Methods
        public void CheckConnection()
        {
            CheckConnectionAsync().ConfigureAwait(false);
        }

        private async Task CheckConnectionAsync()
        {
            IsChecking = true;

            await Task.Run(async () =>
             {
                 List<Task> tasks = new List<Task>();

                 tasks.Add(CheckConnectionComputerAsync());
                 tasks.Add(CheckConnectionRouterAsync());
                 tasks.Add(CheckConnectionInternetAsync());

                 await Task.WhenAll(tasks);
             });

            IsChecking = false;
        }

        private Task CheckConnectionComputerAsync()
        {
            return Task.Run(() =>
            {
                // Reset variables
                IsComputerIPv4Checking = true;
                ComputerIPv4 = "";
                ComputerIPv4State = ConnectionState.None;
                IsComputerIPv6Checking = true;
                ComputerIPv6 = "";
                ComputerIPv6State = ConnectionState.None;
                IsComputerDNSChecking = true;
                ComputerDNS = "";
                ComputerDNSState = ConnectionState.None;

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
                        ComputerDNSState = ConnectionState.Warning;
                    }
                }

                IsComputerDNSChecking = false;
            });
        }

        private Task CheckConnectionRouterAsync()
        {
            return Task.Run(() =>
            {
                // Reset variables
                IsRouterIPv4Checking = true;
                RouterIPv4 = "";
                RouterIPv4State = ConnectionState.None;
                IsRouterIPv6Checking = true;
                RouterIPv6 = "";
                RouterIPv6State = ConnectionState.None;
                IsRouterDNSChecking = true;
                RouterDNS = "";
                RouterDNSState = ConnectionState.None;

                // Detect router IPv4 and if it is reachable
                // Detect local IPv4 address
                try
                {
                    var computerIPv4 = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicICMPTestIPAddress));
                    RouterIPv4 = Models.Network.NetworkInterface.DetectGatewayBasedOnLocalIPAddress(computerIPv4).ToString();

                    RouterIPv4State = ConnectionState.OK;
                }
                catch (Exception)
                {
                    RouterIPv4 = "-/-";
                    RouterIPv4State = ConnectionState.Critical;
                }

                IsRouterIPv4Checking = false;

                // Detect router IPv6 and if it is reachable
                try
                {
                    var computerIPv6 = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse("2606:4700:4700::1111"));
                    RouterIPv6 = Models.Network.NetworkInterface.DetectGatewayBasedOnLocalIPAddress(computerIPv6).ToString();

                    RouterIPv6State = ConnectionState.OK;
                }
                catch (Exception)
                {
                    RouterIPv6 = "-/-";
                    RouterIPv6State = ConnectionState.Critical;
                }

                IsRouterIPv6Checking = false;

                // Detect router dns
                if (!string.IsNullOrEmpty(RouterIPv4))
                {
                    try
                    {
                        RouterDNS = Dns.GetHostEntry(RouterIPv4).HostName;
                        RouterDNSState = ConnectionState.OK;
                    }
                    catch (SocketException)
                    {
                        RouterDNS = "-/-";
                        RouterDNSState = ConnectionState.Warning;
                    }
                }

                IsRouterDNSChecking = false;
            });
        }

        private Task CheckConnectionInternetAsync()
        {
            return Task.Run(() =>
                {
                    // Reset variables
                    IsInternetIPv4Checking = true;
                    InternetIPv4 = "";
                    InternetIPv4State = ConnectionState.None;
                    IsInternetIPv6Checking = true;
                    InternetIPv6 = "";
                    InternetIPv6State = ConnectionState.None;
                    IsInternetDNSChecking = true;
                    InternetDNS = "";
                    InternetDNSState = ConnectionState.None;

                    // Detect public IPv4 and if it is reachable
                    if (SettingsManager.Current.Dashboard_CheckPublicIPAddress)
                    {
                        var publicIPv4AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPAddressAPI ? SettingsManager.Current.Dashboard_CustomPublicIPAddressAPI : GlobalStaticConfiguration.Dashboard_PublicIPAddressAPI;

                        try
                        {
                            var webClient = new WebClient();
                            var result = webClient.DownloadString(publicIPv4AddressAPI);
                            var match = Regex.Match(result, RegexHelper.IPv4AddressExctractRegex);

                            if (match.Success)
                            {
                                InternetIPv4 = match.Value;
                                InternetIPv4State = ConnectionState.OK;
                            }
                            else
                            {
                                InternetIPv4 = "-/-";
                                InternetIPv4State = ConnectionState.Critical;
                            }
                        }
                        catch
                        {
                            InternetIPv4 = "-/-";
                            InternetIPv4State = ConnectionState.Critical;
                        }
                    }
                    else
                    {
                        InternetIPv4 = "Check is disabled";
                        InternetIPv4State = ConnectionState.Info;
                    }

                    IsInternetIPv4Checking = false;

                    // Detect public IPv6 and if it is reachable
                    if (SettingsManager.Current.Dashboard_CheckPublicIPAddress)
                    {
                        var publicIPv6AddressAPI = "https://api6.ipify.org"; // SettingsManager.Current.Dashboard_UseCustomPublicIPAddressAPI ? SettingsManager.Current.Dashboard_CustomPublicIPAddressAPI : GlobalStaticConfiguration.Dashboard_PublicIPAddressAPI;

                        try
                        {
                            var webClient = new WebClient();
                            var result = webClient.DownloadString(publicIPv6AddressAPI);
                            var match = Regex.Match(result, RegexHelper.IPv6AddressExctractRegex);

                            if (match.Success)
                            {
                                InternetIPv6 = match.Value;
                                InternetIPv6State = ConnectionState.OK;
                            }
                            else
                            {
                                InternetIPv6 = "-/-";
                                InternetIPv6State = ConnectionState.Critical;
                            }
                        }
                        catch
                        {
                            InternetIPv6 = "-/-";
                            InternetIPv6State = ConnectionState.Critical;
                        }
                    }
                    else
                    {
                        InternetIPv6 = "Check is disabled";
                        InternetIPv6State = ConnectionState.Info;
                    }

                    IsInternetIPv6Checking = false;

                    // Detect public dns
                    if (!string.IsNullOrEmpty(InternetIPv4))
                    {
                        try
                        {
                            InternetDNS = Dns.GetHostEntry(InternetIPv4).HostName;
                            InternetDNSState = ConnectionState.OK;
                        }
                        catch (SocketException)
                        {
                            InternetDNS = "-/-";
                            InternetDNSState = ConnectionState.Warning;
                        }
                    }

                    IsInternetDNSChecking = false;
                });
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

