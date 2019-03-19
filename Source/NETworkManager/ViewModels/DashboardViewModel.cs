using NETworkManager.Models.Settings;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Views;
using NETworkManager.Utilities;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

        private bool _isConnectionCheckHostRunning;
        public bool IsConnectionCheckHostRunning
        {
            get => _isConnectionCheckHostRunning;
            set
            {
                if(value == _isConnectionCheckHostRunning)
                    return;

                _isConnectionCheckHostRunning = value;
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

        private string _connectionHostIPAddress;
        public string ConnectionHostIPAddress
        {
            get => _connectionHostIPAddress;
            set
            {
                if (value == _connectionHostIPAddress)
                    return;

                _connectionHostIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _connectionHostHostname;
        public string ConnectionHostHostname
        {
            get => _connectionHostHostname;
            set
            {
                if (value == _connectionHostHostname)
                    return;

                _connectionHostHostname = value;
                OnPropertyChanged();
            }
        }
        
        private List<string> _connectionHostDetails = new List<string>();
        public List<string> ConnectionHostDetails
        {
            get => _connectionHostDetails;
            set
            {
                if (value == _connectionHostDetails)
                    return;

                _connectionHostDetails = value;
                OnPropertyChanged();
            }
        }

        private string _connectionGatewayIPAddress;
        public string ConnectionGatewayIPAddress
        {
            get => _connectionGatewayIPAddress;
            set
            {
                if (value == _connectionGatewayIPAddress)
                    return;

                _connectionGatewayIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _connectionPublicIPAddress;
        public string ConnectionPublicIPAddress
        {
            get => _connectionPublicIPAddress;
            set
            {
                if (value == _connectionPublicIPAddress)
                    return;

                _connectionPublicIPAddress = value;
                OnPropertyChanged();
            }
        }

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

        public void CheckConnection()
        {
            // Reset
            IsConnectionCheckHostRunning = true;
            HostConnectionState = ConnectionState.None;
            ConnectionHostIPAddress = "";
            ConnectionHostHostname = "";
            ConnectionHostDetails.Clear();
            
            ConnectionGatewayIPAddress = "";
            
            ConnectionPublicIPAddress = "";

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
                        ConnectionHostDetails.Add("[OK] tcp/ip stack - 127.0.0.1 is reachable via ICMP!");

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
                ConnectionHostDetails.Add("[Error] tcp/ip stack - 127.0.0.1 is not reachable via ICMP! Check your network card...");

                return;
            }


            // 2) Detect the local ip address
            var hostIPAddressDetected = NetworkInterface.DetectLocalIPAddressBasedOnRouting(IPAddress.Parse(SettingsManager.Current.Dashboard_PublicIPAddress));

            if (hostIPAddressDetected == null)
            {
                HostConnectionState = ConnectionState.Error;
                ConnectionHostDetails.Add("[Error] ip address - Could not detect local ip address!");

                return;
            }

            ConnectionHostDetails.Add($"[OK] ip address - Detected local ip address is {hostIPAddressDetected}");

            ConnectionHostIPAddress = hostIPAddressDetected.ToString();

            // 3) Check local dns entry 
            // Warn if not found!
            try
            {
                var hostHostname = Dns.GetHostEntry(hostIPAddressDetected).HostName;
                
                ConnectionHostHostname = hostHostname;
                ConnectionHostDetails.Add($"[OK] dns - Hostname {hostHostname} resolved for ip address {hostIPAddressDetected}!");
            }
            catch (SocketException)
            {
                HostConnectionState = ConnectionState.Warning;
                ConnectionHostDetails.Add($"[Error] dns - Could not resolve hostname for ip address {hostIPAddressDetected}!");
            }

            IsConnectionCheckHostRunning = false;

            // 4) Detect the gateway ip address
            var gatewayIPAddressDetected = NetworkInterface.DetectGatewayBasedOnLocalIPAddress(hostIPAddressDetected);

            if (gatewayIPAddressDetected == null)
            {
                // new var --> HostConnectionState = ConnectionState.Error;
                ConnectionHostDetails.Add("[Error] gateway - Could not detect gateway ip address!");

                return;
            }

            ConnectionGatewayIPAddress = gatewayIPAddressDetected.ToString();
            
            // 4) Check gateway --> ICMP to gateway ip
            // 5) Check gateway dns entry?
            // 6) Check public ip via icmp
            // 7) Check public dns
            // 8) Check public ip address agains api.ipify
            var webClient = new WebClient();
            ConnectionPublicIPAddress = webClient.DownloadString("https://api.ipify.org");
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
