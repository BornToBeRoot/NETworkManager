using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
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

        CancellationTokenSource tokenSource;
        CancellationToken ct;

        private async Task CheckConnectionAsync()
        {
            // Already in queue
            if (tokenSource != null && tokenSource.IsCancellationRequested)
            {                
                return;
            }

            // Cancel if running
            if (IsChecking)
            {
                tokenSource.Cancel();                

                while (IsChecking)
                {
                    await Task.Delay(250);
                }
            }

            // Start check
            IsChecking = true;

            tokenSource = new CancellationTokenSource();
            ct = tokenSource.Token;

            try
            {
                await Task.Run(async () =>
                 {
                     List<Task> tasks = new()
                     {
                         CheckConnectionComputerAsync(ct),
                         CheckConnectionRouterAsync(ct),
                         CheckConnectionInternetAsync(ct)
                     };

                     await Task.WhenAll(tasks);
                 }, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                tokenSource.Dispose();
                IsChecking = false;
            }
        }

        private Task CheckConnectionComputerAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                // Init variables
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
                var detectedLocalIPv4Address = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv4Address));

                if (detectedLocalIPv4Address != null)
                {
                    ComputerIPv4 = detectedLocalIPv4Address.ToString();
                    ComputerIPv4State = string.IsNullOrEmpty(ComputerIPv4) ? ConnectionState.Critical : ConnectionState.OK;
                }
                else
                {
                    ComputerIPv4 = "-/-";
                    ComputerIPv4State = ConnectionState.Critical;
                }

                IsComputerIPv4Checking = false;

                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

                // Detect local IPv6 address
                var detectedLocalIPv6Address = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv6Address));

                if (detectedLocalIPv6Address != null)
                {
                    ComputerIPv6 = detectedLocalIPv6Address.ToString();
                    ComputerIPv6State = string.IsNullOrEmpty(ComputerIPv6) ? ConnectionState.Critical : ConnectionState.OK;
                }
                else
                {
                    ComputerIPv6 = "-/-";
                    ComputerIPv6State = ConnectionState.Critical;
                }

                IsComputerIPv6Checking = false;

                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

                // Try to resolve local DNS based on IPv4
                if (ComputerIPv4State == ConnectionState.OK)
                {
                    var dnsResult = await DNS.GetInstance().ResolvePtrAsync(IPAddress.Parse(ComputerIPv4));

                    if(!dnsResult.HasError)
                    {
                        ComputerDNS = dnsResult.Value;
                        ComputerDNSState = ConnectionState.OK;
                    }
                }

                // Try to resolve local DNS based on IPv6 if IPv4 failed
                if(string.IsNullOrEmpty(ComputerDNS) && ComputerIPv6State == ConnectionState.OK)
                {
                    var dnsResult = await DNS.GetInstance().ResolvePtrAsync(IPAddress.Parse(ComputerIPv6));

                    if (!dnsResult.HasError)
                    {
                        ComputerDNS = dnsResult.Value;
                        ComputerDNSState = ConnectionState.OK;
                    }
                }

                if (string.IsNullOrEmpty(ComputerDNS))
                {
                    ComputerDNS = "-/-";
                    ComputerDNSState = ConnectionState.Critical;
                }

                IsComputerDNSChecking = false;
            }, ct);
        }

        private Task CheckConnectionRouterAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                // Init variables
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
                var detectedLocalIPv4Address = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv4Address));

                if (detectedLocalIPv4Address != null)
                {
                    var detectedRouterIPv4 = Models.Network.NetworkInterface.DetectGatewayBasedOnLocalIPAddress(detectedLocalIPv4Address);

                    if (detectedRouterIPv4 != null)
                    {
                        RouterIPv4 = detectedRouterIPv4.ToString();
                        RouterIPv4State = string.IsNullOrEmpty(RouterIPv4) ? ConnectionState.Critical : ConnectionState.OK;
                    }
                    else
                    {
                        RouterIPv4 = "-/-";
                        RouterIPv4State = ConnectionState.Critical;
                    }
                }
                else
                {
                    RouterIPv4 = "-/-";
                    RouterIPv4State = ConnectionState.Critical;
                }

                IsRouterIPv4Checking = false;

                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();

                // Detect router IPv6 and if it is reachable
                var detectedComputerIPv6 = Models.Network.NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPv6Address));

                if (detectedComputerIPv6 != null)
                {
                    var detectedRouterIPv6 = Models.Network.NetworkInterface.DetectGatewayBasedOnLocalIPAddress(detectedComputerIPv6);

                    if (detectedRouterIPv6 != null)
                    {
                        RouterIPv6 = detectedRouterIPv6.ToString();
                        RouterIPv6State = string.IsNullOrEmpty(RouterIPv6) ? ConnectionState.Critical : ConnectionState.OK;
                    }
                    else
                    {
                        RouterIPv6 = "-/-";
                        RouterIPv6State = ConnectionState.Critical;
                    }
                }
                else
                {
                    RouterIPv6 = "-/-";
                    RouterIPv6State = ConnectionState.Critical;
                }

                IsRouterIPv6Checking = false;

                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                                
                // Try to resolve router DNS based on IPv4
                if (RouterIPv4State == ConnectionState.OK)
                {
                    var dnsResult = await DNS.GetInstance().ResolvePtrAsync(IPAddress.Parse(RouterIPv4));

                    if (!dnsResult.HasError)
                    {
                        RouterDNS = dnsResult.Value;
                        RouterDNSState = ConnectionState.OK;
                    }
                }

                // Try to resolve router DNS based on IPv6 if IPv4 failed
                if (string.IsNullOrEmpty(RouterDNS) && RouterIPv6State == ConnectionState.OK)
                {
                    var dnsResult = await DNS.GetInstance().ResolvePtrAsync(IPAddress.Parse(RouterIPv6));

                    if (!dnsResult.HasError)
                    {
                        RouterDNS = dnsResult.Value;
                        RouterDNSState = ConnectionState.OK;
                    }
                }

                if (string.IsNullOrEmpty(RouterDNS))
                {
                    RouterDNS = "-/-";
                    RouterDNSState = ConnectionState.Critical;
                }

                IsRouterDNSChecking = false;
            }, ct);
        }

        private Task CheckConnectionInternetAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
                {
                    // Init variables
                    IsInternetIPv4Checking = true;
                    InternetIPv4 = "";
                    InternetIPv4State = ConnectionState.None;
                    IsInternetIPv6Checking = true;
                    InternetIPv6 = "";
                    InternetIPv6State = ConnectionState.None;
                    IsInternetDNSChecking = true;
                    InternetDNS = "";
                    InternetDNSState = ConnectionState.None;

                    // If public IP address check is disabled
                    if (!SettingsManager.Current.Dashboard_CheckPublicIPAddress)
                    {
                        InternetIPv4 = Strings.CheckIsDisabled;
                        InternetIPv4State = ConnectionState.Info;
                        IsInternetIPv4Checking = false;

                        InternetIPv6 = Strings.CheckIsDisabled;
                        InternetIPv6State = ConnectionState.Info;
                        IsInternetIPv6Checking = false;

                        InternetDNS = Strings.CheckIsDisabled;
                        InternetDNSState = ConnectionState.Info;
                        IsInternetDNSChecking = false;

                        return;
                    }

                    // Detect public IPv4 and if it is reachable
                    var publicIPv4AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI ? SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI : GlobalStaticConfiguration.Dashboard_PublicIPv4AddressAPI;

                    try
                    {
                        HttpClient httpClient = new();
                        var httpResponse = await httpClient.GetAsync(publicIPv4AddressAPI);

                        var result = await httpResponse.Content.ReadAsStringAsync();

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

                    IsInternetIPv4Checking = false;

                    if (ct.IsCancellationRequested)
                        ct.ThrowIfCancellationRequested();

                    // Detect public IPv6 and if it is reachable
                    var publicIPv6AddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI ? SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI : GlobalStaticConfiguration.Dashboard_PublicIPv6AddressAPI;

                    try
                    {
                        HttpClient httpClient = new();
                        var httpResponse = await httpClient.GetAsync(publicIPv4AddressAPI);

                        var result = await httpResponse.Content.ReadAsStringAsync();

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

                    IsInternetIPv6Checking = false;

                    if (ct.IsCancellationRequested)
                        ct.ThrowIfCancellationRequested();
                                        
                    // Try to resolve public DNS based on IPv4
                    if (InternetIPv4State == ConnectionState.OK)
                    {
                        var dnsResult = await DNS.GetInstance().ResolvePtrAsync(IPAddress.Parse(InternetIPv4));

                        if (!dnsResult.HasError)
                        {
                            InternetDNS = dnsResult.Value;
                            InternetDNSState = ConnectionState.OK;
                        }
                    }

                    // Try to resolve router DNS based on IPv6 if IPv4 failed
                    if (string.IsNullOrEmpty(InternetDNS) && InternetIPv6State == ConnectionState.OK)
                    {
                        var dnsResult = await DNS.GetInstance().ResolvePtrAsync(IPAddress.Parse(InternetIPv6));

                        if (!dnsResult.HasError)
                        {
                            InternetDNS = dnsResult.Value;
                            InternetDNSState = ConnectionState.OK;
                        }
                    }

                    if (string.IsNullOrEmpty(InternetDNS))
                    {
                        InternetDNS = "-/-";
                        InternetDNSState = ConnectionState.Critical;
                    }
                    
                    IsInternetDNSChecking = false;
                }, ct);
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

