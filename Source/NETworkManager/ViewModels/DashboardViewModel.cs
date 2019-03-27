using NETworkManager.Models.Settings;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Views;
using NETworkManager.Utilities;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NETworkManager.Utilities.Enum;
using NetworkInterface = NETworkManager.Models.Network.NetworkInterface;

namespace NETworkManager.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        #region  Variables 
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;

        #region Connection check
        #region Host
        private bool _isHostCheckRunning;
        public bool IsHostCheckRunning
        {
            get => _isHostCheckRunning;
            set
            {
                if (value == _isHostCheckRunning)
                    return;

                _isHostCheckRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isHostCheckComplete;
        public bool IsHostCheckComplete
        {
            get => _isHostCheckComplete;
            set
            {
                if (value == _isHostCheckComplete)
                    return;

                _isHostCheckComplete = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _hostConnectionState = ConnectionState.None;
        public ConnectionState HostConnectionState
        {
            get => _hostConnectionState;
            set
            {
                if (value == _hostConnectionState)
                    return;

                _hostConnectionState = value;
                OnPropertyChanged();
            }
        }

        private string _hostIPAddress;
        public string HostIPAddress
        {
            get => _hostIPAddress;
            set
            {
                if (value == _hostIPAddress)
                    return;

                _hostIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _hostHostname;
        public string HostHostname
        {
            get => _hostHostname;
            set
            {
                if (value == _hostHostname)
                    return;

                _hostHostname = value;
                OnPropertyChanged();
            }
        }

        private string _hostDetails;
        public string HostDetails
        {
            get => _hostDetails;
            set
            {
                if (value == _hostDetails)
                    return;

                _hostDetails = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Host to Gateway / Gateway
        private bool _isGatewayCheckRunning;
        public bool IsGatewayCheckRunning
        {
            get => _isGatewayCheckRunning;
            set
            {
                if (value == _isGatewayCheckRunning)
                    return;

                _isGatewayCheckRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isGatewayCheckComplete;
        public bool IsGatewayCheckComplete
        {
            get => _isGatewayCheckComplete;
            set
            {
                if (value == _isGatewayCheckComplete)
                    return;

                _isGatewayCheckComplete = value;
                OnPropertyChanged();
            }
        }

        private string _gatewayDetails;
        public string GatewayDetails
        {
            get => _gatewayDetails;
            set
            {
                if (value == _gatewayDetails)
                    return;

                _gatewayDetails = value;
                OnPropertyChanged();
            }
        }

        private bool _isGatewayAvailable;
        public bool IsGatewayAvailable
        {
            get => _isGatewayAvailable;
            set
            {
                if (value == _isGatewayAvailable)
                    return;

                _isGatewayAvailable = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _gatewayConnectionState = ConnectionState.None;
        public ConnectionState GatewayConnectionState
        {
            get => _gatewayConnectionState;
            set
            {
                if (value == _gatewayConnectionState)
                    return;

                _gatewayConnectionState = value;
                OnPropertyChanged();
            }
        }

        private string _gatewayIPAddress;
        public string GatewayIPAddress
        {
            get => _gatewayIPAddress;
            set
            {
                if (value == _gatewayIPAddress)
                    return;

                _gatewayIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _gatewayHostname;
        public string GatewayHostname
        {
            get => _gatewayHostname;
            set
            {
                if (value == _gatewayHostname)
                    return;

                _gatewayHostname = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Internet
        private bool _isInternetCheckRunning;
        public bool IsInternetCheckRunning
        {
            get => _isInternetCheckRunning;
            set
            {
                if (value == _isInternetCheckRunning)
                    return;

                _isInternetCheckRunning = value;
                OnPropertyChanged();
            }
        }

        private string _internetDetails;
        public string InternetDetails
        {
            get => _internetDetails;
            set
            {
                if (value == _internetDetails)
                    return;

                _internetDetails = value;
                OnPropertyChanged();
            }
        }

        private bool _isInternetAvailable;
        public bool IsInternetAvailable
        {
            get => _isInternetAvailable;
            set
            {
                if (value == _isInternetAvailable)
                    return;

                _isInternetAvailable = value;
                OnPropertyChanged();
            }
        }

        private ConnectionState _internetConnectionState = ConnectionState.None;
        public ConnectionState InternetConnectionState
        {
            get => _internetConnectionState;
            set
            {
                if (value == _internetConnectionState)
                    return;

                _internetConnectionState = value;
                OnPropertyChanged();
            }
        }


        private string _publicIPAddress;
        public string PublicIPAddress
        {
            get => _publicIPAddress;
            set
            {
                if (value == _publicIPAddress)
                    return;

                _publicIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _publicHostname;
        public string PublicHostname
        {
            get => _publicHostname;
            set
            {
                if (value == _publicHostname)
                    return;

                _publicHostname = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public bool IsCheckConnectionRunning => IsInternetCheckRunning || IsGatewayCheckRunning || IsHostCheckRunning;
        #endregion
        #region Profiles
        public ICollectionView Profiles { get; }

        private ProfileInfo _selectedProfile;
        public ProfileInfo SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (value == _selectedProfile)
                    return;

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                Profiles.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Constructor, load settings
        public DashboardViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            Profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.WakeOnLAN_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name
                return info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            // Detect if network address or status changed...
            NetworkChange.NetworkAvailabilityChanged += (sender, args) => CheckConnectionAsync();
            NetworkChange.NetworkAddressChanged += (sender, args) => CheckConnectionAsync();

            LoadSettings();

            CheckConnectionAsync();

            _isLoading = false;
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions
        public ICommand CheckConnectionCommand
        {
            get { return new RelayCommand(p => CheckConnectionAction(), CheckConnection_CanExecute); }
        }

        private bool CheckConnection_CanExecute(object paramter)
        {
            return !IsCheckConnectionRunning;
        }

        private void CheckConnectionAction()
        {
            CheckConnectionAsync();
        }

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups());

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileCommand
        {
            get { return new RelayCommand(p => EditProfileAction()); }
        }

        private async void EditProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RemoveProfile(SelectedProfile);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups(), true, SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopyAsProfileCommand
        {
            get { return new RelayCommand(p => CopyAsProfileAction()); }
        }

        private async void CopyAsProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.CopyProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.AddProfile(instance);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, ProfileManager.GetGroups(), false, SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteProfileCommand
        {
            get { return new RelayCommand(p => DeleteProfileAction()); }
        }

        private async void DeleteProfileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.DeleteProfile
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RemoveProfile(SelectedProfile);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Resources.Localization.Strings.DeleteProfileMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private async void EditGroupAction(object group)
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditGroup
            };

            var editGroupViewModel = new GroupViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RenameGroup(instance.OldGroup, instance.Group);

                Profiles.Refresh();
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, group.ToString(), ProfileManager.GetGroups());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }
        #endregion

        #region Methods
        public void CheckConnectionAsync()
        {
            Task.Run(() => CheckConnection());
        }

        public void CheckConnection()
        {
            if (IsCheckConnectionRunning)
                return;

            // Reset
            IsHostCheckRunning = true;
            IsHostCheckComplete = false;
            HostDetails = "";
            HostConnectionState = ConnectionState.None;
            HostIPAddress = "";
            HostHostname = "";

            IsGatewayCheckRunning = true;
            IsGatewayCheckComplete = false;
            GatewayDetails = "";
            IsGatewayAvailable = false;
            GatewayConnectionState = ConnectionState.None;
            GatewayIPAddress = "";
            GatewayHostname = "";

            IsInternetCheckRunning = true;
            InternetDetails = "";
            IsInternetAvailable = false;
            InternetConnectionState = ConnectionState.None;
            PublicIPAddress = "";
            PublicHostname = "";

            // 1) Check tcp/ip stack --> ICMP to 127.0.0.1
            var localhostIPAddress = "127.0.0.1";

            using (var ping = new Ping())
            {
                for (var i = 0; i < 2; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(IPAddress.Parse(localhostIPAddress));

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        HostConnectionState = ConnectionState.OK;
                        AddToHostDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.TCPIPStackIsAvailableMessage, localhostIPAddress));

                        break;
                    }
                    catch (PingException)
                    {

                    }
                }
            }

            if (HostConnectionState == ConnectionState.None)
            {
                HostConnectionState = ConnectionState.Error;
                AddToHostDetails(ConnectionState.Error, string.Format(Resources.Localization.Strings.TCPIPStackIsAvailableMessage, localhostIPAddress));

                return;
            }

            // 2) Detect the local ip address
            IPAddress hostIPAddressDetected;

            try
            {
                hostIPAddressDetected = NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPAddress));

                if (hostIPAddressDetected == null)
                {
                    HostConnectionState = ConnectionState.Error;
                    AddToHostDetails(ConnectionState.Error, Resources.Localization.Strings.CouldNotDetectLocalIPAddressMessage + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndNetworkConnectionMessage);

                    IsHostCheckRunning = false;
                    IsGatewayCheckRunning = false;
                    IsInternetCheckRunning = false;

                    return;
                }
            }
            catch (Exception)
            {
                HostConnectionState = ConnectionState.Error;
                AddToHostDetails(ConnectionState.Error, Resources.Localization.Strings.CouldNotDetectLocalIPAddressMessage + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndNetworkConnectionMessage);

                IsHostCheckRunning = false;
                IsGatewayCheckRunning = false;
                IsInternetCheckRunning = false;

                return;
            }

            HostIPAddress = hostIPAddressDetected.ToString();
            AddToHostDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.XXXDetectedAsLocalIPAddressMessage, HostIPAddress));

            // 3) Check dns for local host
            try
            {
                var hostHostname = Dns.GetHostEntry(hostIPAddressDetected).HostName;

                HostHostname = hostHostname;
                AddToHostDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.HostnameXXXResolvedForIPAddressXXXMessage, HostHostname, HostIPAddress));
            }
            catch (SocketException)
            {
                HostConnectionState = ConnectionState.Warning;
                AddToHostDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.CouldNotResolveHostnameForXXXMessage, HostIPAddress) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            IsHostCheckRunning = false;
            IsHostCheckComplete = true;

            IPAddress gatewayIPAddressDetected;

            // 4) Detect gateway ip address
            try
            {
                gatewayIPAddressDetected = NetworkInterface.DetectGatewayBasedOnLocalIPAddress(hostIPAddressDetected);

                // CANCEL
                if (gatewayIPAddressDetected == null)
                {
                    AddToGatewayDetails(ConnectionState.Error, Resources.Localization.Strings.CouldNotDetectGatewayIPAddressMessage + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndNetworkConnectionMessage);

                    IsGatewayCheckRunning = false;
                    IsInternetCheckRunning = false;

                    return;
                }
            }
            catch (Exception)
            {
                AddToGatewayDetails(ConnectionState.Error, Resources.Localization.Strings.CouldNotDetectGatewayIPAddressMessage + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndNetworkConnectionMessage);

                IsGatewayCheckRunning = false;
                IsInternetCheckRunning = false;

                return;
            }

            GatewayIPAddress = gatewayIPAddressDetected.ToString();
            AddToGatewayDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.XXXDetectedAsGatewayIPAddress, GatewayIPAddress));

            // 4) Check if gateway is reachable via ICMP
            using (var ping = new Ping())
            {
                for (var i = 0; i < 2; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(GatewayIPAddress);

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        IsGatewayAvailable = true;
                        AddToGatewayDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.XXXIsReachableViaICMPMessage, GatewayIPAddress));

                        break;
                    }
                    catch (PingException)
                    {

                    }
                }
            }

            // CANCEL
            if (!IsGatewayAvailable)
            {
                AddToGatewayDetails(ConnectionState.Error, string.Format(Resources.Localization.Strings.XXXIsNotReachableViaICMPMessage, GatewayIPAddress));

                IsGatewayCheckRunning = false;
                IsInternetCheckRunning = false;

                return;
            }

            GatewayConnectionState = ConnectionState.OK;

            // 5) Check gateway dns entry
            try
            {
                var gatewayHostname = Dns.GetHostEntry(GatewayIPAddress).HostName;

                GatewayHostname = gatewayHostname;
                AddToGatewayDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.HostnameXXXResolvedForIPAddressXXXMessage, GatewayHostname, GatewayIPAddress));
            }
            catch (SocketException)
            {
                GatewayConnectionState = ConnectionState.Warning;
                AddToGatewayDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.CouldNotResolveHostnameForXXXMessage, GatewayIPAddress) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            IsGatewayCheckRunning = false;
            IsGatewayCheckComplete = true;

            // 6) Check a public ip via icmp
            var internetIPAddress = "1.1.1.1";

            using (var ping = new Ping())
            {
                for (var i = 0; i < 2; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(IPAddress.Parse(internetIPAddress));

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        IsInternetAvailable = true;
                        AddToInternetDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.XXXIsReachableViaICMPMessage, internetIPAddress));

                        break;
                    }
                    catch (PingException)
                    {

                    }
                }
            }

            if (!IsInternetAvailable)
            {
                AddToInternetDetails(ConnectionState.Error, string.Format(Resources.Localization.Strings.XXXIsNotReachableViaICMPMessage, internetIPAddress));

                IsInternetCheckRunning = false;

                return;
            }

            // If internet is reachable via icmp
            InternetConnectionState = ConnectionState.OK;

            // 7) Check public dns (A/AAAA) - Check if dns is working...
            var internetDNSDomain = "one.one.one.one";

            try
            {
                var dnsCount = Dns.GetHostEntry(internetDNSDomain).AddressList.Length;

                if (dnsCount > 0)
                {
                    AddToInternetDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.GotXAorAAAADNSRecordsForXXXMessage, dnsCount, internetDNSDomain));
                }
                else
                {
                    InternetConnectionState = ConnectionState.Warning;
                    AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.GotNoAorAAAADNSRecordsForXXXMessage, internetDNSDomain) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
                }
            }
            catch (SocketException)
            {
                InternetConnectionState = ConnectionState.Warning;
                AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.GotNoAorAAAADNSRecordsForXXXMessage, internetDNSDomain) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            // 8) Check public dns (PTR) - Check if dns is working...
            var internetDNSIPAddress = "1.1.1.1";

            try
            {
                var dnsCount = Dns.GetHostEntry(IPAddress.Parse(internetDNSIPAddress)).AddressList.Length;

                if (dnsCount > 0)
                {
                    AddToInternetDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.GotXPTRDNSRecordsForXXXMessage, dnsCount, internetDNSIPAddress));
                }
                else
                {
                    InternetConnectionState = ConnectionState.Warning;
                    AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.GotNoPTRDNSRecordsForXXXMessage, internetDNSDomain) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
                }
            }
            catch (SocketException)
            {
                InternetConnectionState = ConnectionState.Warning;
                AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.GotNoPTRDNSRecordsForXXXMessage, internetDNSDomain) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            // 9) Check public ip address agains api.ipify
            if (SettingsManager.Current.Dashboard_CheckPublicIPAddress)
            {
                try
                {
                    var webClient = new WebClient();
                    var publicIPAdressResult = webClient.DownloadString(SettingsManager.Current.Dashboard_PublicIPAddressAPI);
                    var ipv4Regex = new Regex(RegexHelper.IPv4AddressRegex);
                    var publicIPAddress = ipv4Regex.Match(publicIPAdressResult).Value;

                    if (string.IsNullOrEmpty(publicIPAddress))
                    {
                        InternetConnectionState = ConnectionState.Warning;
                        AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.CouldNotGetPublicIPAddressFromXXXMessage, SettingsManager.Current.Dashboard_PublicIPAddressAPI));
                    }

                    PublicIPAddress = publicIPAddress;
                    AddToInternetDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.GotXXXAsPublicIPAddressFromXXXMessage, PublicIPAddress, SettingsManager.Current.Dashboard_PublicIPAddressAPI));
                }
                catch (Exception)
                {
                    InternetConnectionState = ConnectionState.Warning;
                    AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.CouldNotGetPublicIPAddressFromXXXMessage, SettingsManager.Current.Dashboard_PublicIPAddressAPI));
                }

                // 10) Resolve dns for public ip
                if (!string.IsNullOrEmpty(PublicIPAddress))
                {
                    try
                    {
                        var publicHostname = Dns.GetHostEntry(PublicIPAddress).HostName;

                        PublicHostname = publicHostname;
                        AddToInternetDetails(ConnectionState.OK, string.Format(Resources.Localization.Strings.HostnameXXXResolvedForIPAddressXXXMessage, PublicHostname, PublicIPAddress));
                    }
                    catch (SocketException)
                    {
                        InternetConnectionState = ConnectionState.Warning;
                        AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.CouldNotResolveHostnameForXXXMessage, PublicIPAddress) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
                    }
                }
            }
            else
            {
                // Show note that public ip check is disabled
                PublicIPAddress = Resources.Localization.Strings.PublicIPAddressCheckIsDisabled.Replace("\n", "").Split('\r')[0];
                PublicHostname = Resources.Localization.Strings.PublicIPAddressCheckIsDisabled.Replace("\n", "").Split('\r')[1];
            }

            IsInternetCheckRunning = false;
        }

        public void AddToHostDetails(ConnectionState state, string message)
        {
            if (!string.IsNullOrEmpty(HostDetails))
                HostDetails += Environment.NewLine;

            HostDetails += $"[{LocalizationManager.TranslateConnectionState(state)}] {message}";
        }

        public void AddToGatewayDetails(ConnectionState state, string message)
        {
            if (!string.IsNullOrEmpty(GatewayDetails))
                GatewayDetails += Environment.NewLine;

            GatewayDetails += $"[{LocalizationManager.TranslateConnectionState(state)}] {message}";
        }

        public void AddToInternetDetails(ConnectionState state, string message)
        {
            if (!string.IsNullOrEmpty(InternetDetails))
                InternetDetails += Environment.NewLine;

            InternetDetails += $"[{LocalizationManager.TranslateConnectionState(state)}] {message}";
        }

        public void OnViewVisible()
        {
            // Refresh profiles
            // Profiles.Refresh();
        }

        public void OnViewHide()
        {

        }
        #endregion
    }
}
