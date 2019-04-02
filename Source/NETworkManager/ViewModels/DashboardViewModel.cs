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


        private bool _isHostReachable;
        public bool IsHostReachable
        {
            get => _isHostReachable;
            set
            {
                if (value == _isHostReachable)
                    return;

                _isHostReachable = value;
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

        private IPAddress _hostIPAddress;
        public IPAddress HostIPAddress
        {
            get => _hostIPAddress;
            set
            {
                if (Equals(value, _hostIPAddress))
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

        #region Gateway / Router
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

        private bool _isGatewayReachable;
        public bool IsGatewayReachable
        {
            get => _isGatewayReachable;
            set
            {
                if (value == _isGatewayReachable)
                    return;

                _isGatewayReachable = value;
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

        private IPAddress _gatewayIPAddress;
        public IPAddress GatewayIPAddress
        {
            get => _gatewayIPAddress;
            set
            {
                if (Equals(value, _gatewayIPAddress))
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

        private bool _isInternetCheckComplete;
        public bool IsInternetCheckComplete
        {
            get => _isInternetCheckComplete;
            set
            {
                if (value == _isInternetCheckComplete)
                    return;

                _isInternetCheckComplete = value;
                OnPropertyChanged();
            }
        }

        private bool _isInternetReachable;
        public bool IsInternetReachable
        {
            get => _isInternetReachable;
            set
            {
                if (value == _isInternetReachable)
                    return;

                _isInternetReachable = value;
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


        private IPAddress _publicIPAddress;
        public IPAddress PublicIPAddress
        {
            get => _publicIPAddress;
            set
            {
                if (Equals(value, _publicIPAddress))
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
        #endregion
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
                    return true;

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

        public ICommand CheckConnectionCommand => new RelayCommand(p => CheckConnectionAction(), CheckConnection_CanExecute);

        private bool CheckConnection_CanExecute(object paramter)
        {
            return !IsInternetCheckRunning;
        }

        private void CheckConnectionAction()
        {
            CheckConnectionAsync();
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction());

        private void EditProfileAction()
        {
            EditProfile();
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction());

        private void CopyAsProfileAction()
        {
            CopyAsProfile();
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction());

        private void DeleteProfileAction()
        {
            DeleteProfile();
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private void EditGroupAction(object group)
        {
            EditGroup(group);
        }

        public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }

        public ICommand RedirectProfileToApplicationCommand => new RelayCommand(RedirectProfileToApplicationAction);

        private void RedirectProfileToApplicationAction(object name)
        {
            if (!(name is string appName))
                return;

            if (!Enum.TryParse(appName, out ApplicationViewManager.Name applicationName))
                return;
            
            EventSystem.RedirectProfileToApplication(applicationName, SelectedProfile);
        }
        #endregion

        #region Methods
        public void CheckConnectionAsync()
        {
            Task.Run(() => CheckConnection());
        }

        public void CheckConnection()
        {
            if (IsInternetCheckRunning)
                return;

            // Reset
            IsHostCheckRunning = true;
            IsHostCheckComplete = false;
            HostDetails = "";
            IsHostReachable = false;
            HostConnectionState = ConnectionState.None;
            HostIPAddress = null;
            HostHostname = "";

            IsGatewayCheckRunning = true;
            IsGatewayCheckComplete = false;
            GatewayDetails = "";
            IsGatewayReachable = false;
            GatewayConnectionState = ConnectionState.None;
            GatewayIPAddress = null;
            GatewayHostname = "";

            IsInternetCheckRunning = true;
            IsInternetCheckComplete = false;
            InternetDetails = "";
            IsInternetReachable = false;
            InternetConnectionState = ConnectionState.None;
            PublicIPAddress = null;
            PublicHostname = "";

            #region Host

            // 1) Check tcp/ip stack --> Ping to 127.0.0.1
            var hostIPAddress = "127.0.0.1";

            using (var ping = new Ping())
            {
                for (var i = 0; i < 2; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(IPAddress.Parse(hostIPAddress));

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        IsHostReachable = true;

                        break;
                    }
                    catch (PingException)
                    {

                    }
                }
            }

            if (!IsHostReachable)
            {
                HostConnectionState = ConnectionState.Error;
                AddToHostDetails(ConnectionState.Error,
                    string.Format(Resources.Localization.Strings.TCPIPStackIsNotAvailableMessage, hostIPAddress));

                IsHostCheckRunning = false;
                IsGatewayCheckRunning = false;
                IsInternetCheckRunning = false;

                return;
            }

            HostConnectionState = ConnectionState.OK;
            AddToHostDetails(ConnectionState.OK,
                string.Format(Resources.Localization.Strings.TCPIPStackIsAvailableMessage, hostIPAddress));

            // 2) Detect local ip address
            try
            {
                HostIPAddress =
                    NetworkInterface.DetectLocalIPAddressBasedOnRouting(
                        IPAddress.Parse(SettingsManager.Current.Dashboard_PublicICMPTestIPAddress));
            }
            catch (Exception)
            {
                // ignored
            }

            if (HostIPAddress == null)
            {
                HostConnectionState = ConnectionState.Error;
                AddToHostDetails(ConnectionState.Error,
                    Resources.Localization.Strings.CouldNotDetectLocalIPAddressMessage + " " + Resources.Localization
                        .Strings.CheckNetworkAdapterConfigurationAndNetworkConnectionMessage);

                IsHostCheckRunning = false;
                IsGatewayCheckRunning = false;
                IsInternetCheckRunning = false;

                return;
            }

            AddToHostDetails(ConnectionState.OK,
                string.Format(Resources.Localization.Strings.XXXDetectedAsLocalIPAddressMessage, HostIPAddress));

            // 3) Check dns for local host
            try
            {
                HostHostname = Dns.GetHostEntry(HostIPAddress).HostName;

                AddToHostDetails(ConnectionState.OK,
                    string.Format(Resources.Localization.Strings.ResolvedXXXAsHostnameForIPAddressXXXMessage,
                        HostHostname, HostIPAddress));
            }
            catch (SocketException)
            {
                HostConnectionState = ConnectionState.Warning;
                AddToHostDetails(ConnectionState.Warning,
                    string.Format(Resources.Localization.Strings.CouldNotResolveHostnameForXXXMessage, HostIPAddress) +
                    " " + Resources.Localization.Strings
                        .CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            IsHostCheckRunning = false;
            IsHostCheckComplete = true;

            #endregion

            #region Gateway / Router

            // 4) Detect gateway ip address
            try
            {
                GatewayIPAddress = NetworkInterface.DetectGatewayBasedOnLocalIPAddress(HostIPAddress);
            }
            catch (Exception)
            {
                // ignored
            }

            if (GatewayIPAddress == null)
            {
                AddToGatewayDetails(ConnectionState.Error,
                    Resources.Localization.Strings.CouldNotDetectGatewayIPAddressMessage + " " + Resources.Localization
                        .Strings.CheckNetworkAdapterConfigurationAndNetworkConnectionMessage);

                IsGatewayCheckRunning = false;
                IsInternetCheckRunning = false;

                return;
            }

            AddToGatewayDetails(ConnectionState.OK,
                string.Format(Resources.Localization.Strings.XXXDetectedAsGatewayIPAddress, GatewayIPAddress));

            // 4) Check if gateway is reachable via ICMP
            using (var ping = new Ping())
            {
                for (var i = 0; i < 1; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(GatewayIPAddress);

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        IsGatewayReachable = true;

                        break;
                    }
                    catch (PingException)
                    {
                        // ignore
                    }
                }
            }

            if (!IsGatewayReachable)
            {
                AddToGatewayDetails(ConnectionState.Error,
                    string.Format(Resources.Localization.Strings.XXXIsNotReachableViaICMPMessage, GatewayIPAddress));

                IsGatewayCheckRunning = false;
                IsInternetCheckRunning = false;

                return;
            }

            GatewayConnectionState = ConnectionState.OK;
            AddToGatewayDetails(ConnectionState.OK,
                string.Format(Resources.Localization.Strings.XXXIsReachableViaICMPMessage, GatewayIPAddress));

            // 5) Check dns for gateway
            try
            {
                GatewayHostname = Dns.GetHostEntry(GatewayIPAddress).HostName;

                AddToGatewayDetails(ConnectionState.OK,
                    string.Format(Resources.Localization.Strings.ResolvedXXXAsHostnameForIPAddressXXXMessage,
                        GatewayHostname, GatewayIPAddress));
            }
            catch (SocketException)
            {
                GatewayConnectionState = ConnectionState.Warning;
                AddToGatewayDetails(ConnectionState.Warning,
                    string.Format(Resources.Localization.Strings.CouldNotResolveHostnameForXXXMessage,
                        GatewayIPAddress) + " " + Resources.Localization.Strings
                        .CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            IsGatewayCheckRunning = false;
            IsGatewayCheckComplete = true;

            #endregion

            #region Internet

            // 6) Check if internet is reachable via icmp to a public ip address
            var publicICMPTestIPAddress = SettingsManager.Current.Dashboard_PublicICMPTestIPAddress;

            using (var ping = new Ping())
            {
                for (var i = 0; i < 1; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(IPAddress.Parse(publicICMPTestIPAddress));

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        IsInternetReachable = true;

                        break;
                    }
                    catch (PingException)
                    {

                    }
                }
            }

            if (!IsInternetReachable)
            {
                AddToInternetDetails(ConnectionState.Error,
                    string.Format(Resources.Localization.Strings.XXXIsNotReachableViaICMPMessage,
                        publicICMPTestIPAddress));

                IsInternetCheckRunning = false;

                return;
            }

            InternetConnectionState = ConnectionState.OK;
            AddToInternetDetails(ConnectionState.OK,
                string.Format(Resources.Localization.Strings.XXXIsReachableViaICMPMessage, publicICMPTestIPAddress));

            // 7) Check if dns is working (A)
            var publicDNSTestDomain = SettingsManager.Current.Dashboard_PublicDNSTestDomain;
            var dnsCountForward = 0;

            try
            {
                foreach (var ipAddress in Dns.GetHostEntry(publicDNSTestDomain).AddressList)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        dnsCountForward++;
                }
            }
            catch (SocketException)
            {
                // ignore
            }

            if (dnsCountForward > 0)
            {
                AddToInternetDetails(ConnectionState.OK,
                    string.Format(Resources.Localization.Strings.XADNSRecordsResolvedForXXXMessage, dnsCountForward,
                        publicDNSTestDomain));
            }
            else
            {
                InternetConnectionState = ConnectionState.Warning;
                AddToInternetDetails(ConnectionState.Warning,
                    string.Format(Resources.Localization.Strings.NoADNSRecordsResolvedForXXXMessage,
                        publicDNSTestDomain) + " " + Resources.Localization.Strings
                        .CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            // 8) Check if dns is working (PTR)
            var publicDNSTestIPAddress = SettingsManager.Current.Dashboard_PublicDNSTestIPAddress;
            var gotDnsReverseHostname = false;

            try
            {
                gotDnsReverseHostname =
                    !string.IsNullOrEmpty(Dns.GetHostEntry(IPAddress.Parse(publicDNSTestIPAddress)).HostName);
            }
            catch (SocketException)
            {
                // ignore
            }

            if (gotDnsReverseHostname)
            {
                AddToInternetDetails(ConnectionState.OK,
                    string.Format(Resources.Localization.Strings.PTRDNSRecordResolvedForXXXMessage,
                        publicDNSTestIPAddress));
            }
            else
            {
                InternetConnectionState = ConnectionState.Warning;
                AddToInternetDetails(ConnectionState.Warning,
                    string.Format(Resources.Localization.Strings.NoPTRDNSRecordResolvedForXXXMessage,
                        publicDNSTestDomain) + " " + Resources.Localization.Strings
                        .CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);
            }

            // 9) Check public ip address via api.ipify.org
            var publicIPAddressAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPAddressAPI
                ? SettingsManager.Current.Dashboard_CustomPublicIPAddressAPI
                : GlobalStaticConfiguration.Dashboard_PublicIPAddressAPI;
            var publicIPAddress = "";

            if (SettingsManager.Current.Dashboard_CheckPublicIPAddress)
            {
                try
                {
                    var webClient = new WebClient();
                    var result = webClient.DownloadString(publicIPAddressAPI);
                    var match = Regex.Match(result, RegexHelper.IPv4AddressExctractRegex);

                    if (match.Success)
                    {
                        publicIPAddress = match.Value;
                    }
                    else
                    {
                        InternetConnectionState = ConnectionState.Warning;
                        AddToInternetDetails(ConnectionState.Warning,
                            string.Format(Resources.Localization.Strings.CouldNotParsePublicIPAddressFromXXXMessage,
                                publicIPAddressAPI));

                        IsInternetCheckRunning = false;

                        return;
                    }
                }
                catch (WebException)
                {
                    InternetConnectionState = ConnectionState.Warning;
                    AddToInternetDetails(ConnectionState.Warning,
                        string.Format(Resources.Localization.Strings.CouldNotConnectToXXXMessage, publicIPAddressAPI));

                    IsInternetCheckRunning = false;

                    return;
                }
                catch (Exception)
                {
                    // ignore     
                }

                if (string.IsNullOrEmpty(publicIPAddress))
                {
                    InternetConnectionState = ConnectionState.Warning;
                    AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.CouldNotGetPublicIPAddressFromXXXMessage, publicIPAddressAPI));

                    IsInternetCheckRunning = false;

                    return;
                }

                PublicIPAddress = IPAddress.Parse(publicIPAddress);
                AddToInternetDetails(ConnectionState.OK,
                    string.Format(Resources.Localization.Strings.GotXXXAsPublicIPAddressFromXXXMessage, PublicIPAddress, publicIPAddressAPI));

                // 10) Resolve dns for public ip
                try
                {
                    PublicHostname = Dns.GetHostEntry(PublicIPAddress).HostName;

                    AddToInternetDetails(ConnectionState.OK,
                        string.Format(Resources.Localization.Strings.ResolvedXXXAsHostnameForIPAddressXXXMessage,
                            PublicHostname, PublicIPAddress));
                }
                catch (SocketException)
                {
                    InternetConnectionState = ConnectionState.Warning;
                    AddToInternetDetails(ConnectionState.Warning, string.Format(Resources.Localization.Strings.CouldNotResolveHostnameForXXXMessage, PublicIPAddress) + " " + Resources.Localization.Strings.CheckNetworkAdapterConfigurationAndDNSServerConfigurationMessage);

                    IsInternetCheckRunning = false;

                    return;
                }
            }

            IsInternetCheckRunning = false;
            IsInternetCheckComplete = true;

            #endregion
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

        public async void EditProfile()
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
                }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
                ProfileManager.GetGroups(),
                true, SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void CopyAsProfile()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.CopyProfile
            };

            var profileViewModel = new ProfileViewModel(instance =>
                {
                    _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                    ProfileManager.AddProfile(instance);
                }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
                ProfileManager.GetGroups(),
                false, SelectedProfile);

            customDialog.Content = new ProfileDialog
            {
                DataContext = profileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void DeleteProfile()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.DeleteProfile
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
                {
                    _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                    ProfileManager.RemoveProfile(SelectedProfile);
                }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
                Resources.Localization.Strings.DeleteProfileMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void EditGroup(object group)
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
                }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, group.ToString(),
                ProfileManager.GetGroups());

            customDialog.Content = new GroupDialog
            {
                DataContext = editGroupViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public void OnViewVisible()
        {
            // Refresh profiles
            Profiles.Refresh();
        }

        public void OnViewHide()
        {

        }
        #endregion
    }
}

