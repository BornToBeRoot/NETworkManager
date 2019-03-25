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

            LoadSettings();

            CheckConnectionAsync();

            _isLoading = false;
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions
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

        public async void CheckConnection()
        {
            // Reset
            IsHostCheckRunning = true;
            HostDetails = "";
            HostConnectionState = ConnectionState.None;
            HostIPAddress = "";
            HostHostname = "";

            IsGatewayCheckRunning = true;
            GatewayDetails = "";
            IsGatewayAvailable = false;
            GatewayConnectionState = ConnectionState.None;
            GatewayIPAddress = "";
            GatewayHostname = "";

            PublicIPAddress = "";

            // 1) Check tcp/ip stack --> ICMP to 127.0.0.1
            using (var ping = new Ping())
            {
                for (var i = 0; i < 2; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(IPAddress.Parse("127.0.0.1"));

                        if (pingReply == null || pingReply.Status != IPStatus.Success)
                            continue;

                        HostConnectionState = ConnectionState.OK;
                        AddToHostDetails("[OK] tcp/ip stack - 127.0.0.1 is reachable via ICMP!");

                        break;
                    }
                    catch (PingException)
                    {

                    }
                }
            }

            // If tcp/ip if not available...
            if (HostConnectionState == ConnectionState.None)
            {
                HostConnectionState = ConnectionState.Error;
                AddToHostDetails("[Error] tcp/ip stack - 127.0.0.1 is not reachable via ICMP! Check your network card...");

                return;
            }

            // 2) Detect the local ip address
            var hostIPAddressDetected = NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPAddress));

            if (hostIPAddressDetected == null)
            {
                HostConnectionState = ConnectionState.Error;
                AddToHostDetails("[Error] ip address - Could not detect local ip address!");

                return;
            }

            AddToHostDetails($"[OK] ip address - Detected local ip address is {hostIPAddressDetected}");

            HostIPAddress = hostIPAddressDetected.ToString();

            // 3) Check local dns entry 
            // Warn if not found!
            try
            {
                var hostHostname = Dns.GetHostEntry(hostIPAddressDetected).HostName;

                HostHostname = hostHostname;
                AddToHostDetails($"[OK] dns - Hostname {hostHostname} resolved for ip address {hostIPAddressDetected}!");
            }
            catch (SocketException)
            {
                HostConnectionState = ConnectionState.Warning;
                AddToHostDetails($"[Error] dns - Could not resolve hostname for ip address {hostIPAddressDetected}!");
            }

            IsHostCheckRunning = false;

            // 4) Detect the gateway ip address
            var gatewayIPAddressDetected = NetworkInterface.DetectGatewayBasedOnLocalIPAddress(hostIPAddressDetected);

            if (gatewayIPAddressDetected == null)
            {
                IsGatewayAvailable = false;
                AddToGatewayDetails("[Error] gateway - Could not detect gateway ip address!");
                IsGatewayCheckRunning = false;

                return;
            }

            GatewayIPAddress = gatewayIPAddressDetected.ToString();
            AddToGatewayDetails($"[OK] gateway - Detected gateway ip is {GatewayIPAddress}");

            // 4) Check gateway --> ICMP to gateway ip
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
                        AddToGatewayDetails($"[OK] gateway - {GatewayIPAddress} is reachable via ICMP!");

                        break;
                    }
                    catch (PingException)
                    {

                    }
                }
            }

            if (!IsGatewayAvailable)
            {
                // CANCEL ??? 

                return;
            }

            GatewayConnectionState = ConnectionState.OK;

            // 5) Check gateway dns entry?
            try
            {
                var gatewayHostname = Dns.GetHostEntry(GatewayIPAddress).HostName;

                GatewayHostname = gatewayHostname;
                AddToGatewayDetails($"[OK] dns - Hostname {gatewayHostname} resolved for ip address {GatewayIPAddress}!");
            }
            catch (SocketException)
            {
                GatewayConnectionState = ConnectionState.Warning;
                AddToGatewayDetails($"[Error] dns - Could not resolve hostname for ip address {GatewayIPAddress}!");
            }

            IsGatewayCheckRunning = false;

            // 6) Check public ip via icmp
            // Error

            // 7) Check public dns - Check if dns is working...
            // Warn

            // 8) Check public ip address agains api.ipify
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
                        // warn...
                    }
                    
                    PublicIPAddress = publicIPAddress;

                }
                catch (Exception ex)
                {
                    // warn...
                }
            }
            else
            {
                PublicIPAddress = "Check disabled in settings!";
            }

            // 9) Resolve dns for public ip
            if (!string.IsNullOrEmpty(PublicIPAddress))
            {
                try
                {
                    var publicHostname = Dns.GetHostEntry(PublicIPAddress).HostName;

                    PublicHostname = publicHostname;
                    //  AddToGatewayDetails($"[OK] dns - Hostname {gatewayHostname} resolved for ip address {GatewayIPAddress}!");
                }
                catch (SocketException)
                {
                    // GatewayConnectionState = ConnectionState.Warning;
                    //                AddToGatewayDetails($"[Error] dns - Could not resolve hostname for ip address {GatewayIPAddress}!");
                }
            }
        }

        public void AddToHostDetails(string text)
        {
            if (!string.IsNullOrEmpty(HostDetails))
                HostDetails += Environment.NewLine;

            HostDetails += text;
        }

        public void AddToGatewayDetails(string text)
        {
            if (!string.IsNullOrEmpty(GatewayDetails))
                GatewayDetails += Environment.NewLine;

            GatewayDetails += text;
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
