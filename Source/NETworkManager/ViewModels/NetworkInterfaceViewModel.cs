using System.Collections.Generic;
using System.Windows.Input;
using System.Net.NetworkInformation;
using System;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Models.Network;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using NETworkManager.Views;
using NETworkManager.Utilities;
using System.Windows;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using NETworkManager.Profiles;
using System.Windows.Threading;

namespace NETworkManager.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase, IProfileManager
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _searchDispatcherTimer = new DispatcherTimer();
        private BandwidthMeter _bandwidthMeter;

        private readonly bool _isLoading;

        private bool _isNetworkInteraceLoading;
        public bool IsNetworkInterfaceLoading
        {
            get => _isNetworkInteraceLoading;
            set
            {
                if (value == _isNetworkInteraceLoading)
                    return;

                _isNetworkInteraceLoading = value;
                OnPropertyChanged();
            }
        }

        private bool _canConfigure;
        public bool CanConfigure
        {
            get => _canConfigure;
            set
            {
                if (value == _canConfigure)
                    return;

                _canConfigure = value;
                OnPropertyChanged();
            }
        }

        private bool _isConfigurationRunning;
        public bool IsConfigurationRunning
        {
            get => _isConfigurationRunning;
            set
            {
                if (value == _isConfigurationRunning)
                    return;

                _isConfigurationRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isStatusMessageDisplayed;
        public bool IsStatusMessageDisplayed
        {
            get => _isStatusMessageDisplayed;
            set
            {
                if (value == _isStatusMessageDisplayed)
                    return;

                _isStatusMessageDisplayed = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        #region NetworkInterfaces, SelectedNetworkInterface
        private List<NetworkInterfaceInfo> _networkInterfaces;
        public List<NetworkInterfaceInfo> NetworkInterfaces
        {
            get => _networkInterfaces;
            set
            {
                if (value == _networkInterfaces)
                    return;

                _networkInterfaces = value;
                OnPropertyChanged();
            }
        }

        private NetworkInterfaceInfo _selectedNetworkInterface;
        public NetworkInterfaceInfo SelectedNetworkInterface
        {
            get => _selectedNetworkInterface;
            set
            {
                if (value == _selectedNetworkInterface)
                    return;

                if (value != null)
                {
                    if (!_isLoading)
                        SettingsManager.Current.NetworkInterface_InterfaceId = value.Id;
                                        
                    // Bandwidth
                    StopBandwidthMeter();
                    StartBandwidthMeter(value.Id);

                    // Configuration
                    SetConfigurationDefaults(value);

                    CanConfigure = value.IsOperational;
                }

                _selectedNetworkInterface = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Bandwidth
        private long _bandwidthTotalBytesSentTemp;

        private long _bandwidthTotalBytesSent;
        public long BandwidthTotalBytesSent
        {
            get => _bandwidthTotalBytesSent;
            set
            {
                if (value == _bandwidthTotalBytesSent)
                    return;

                _bandwidthTotalBytesSent = value;
                OnPropertyChanged();
            }
        }

        private long _bandwidthTotalBytesReceivedTemp;
        private long _bandwidthTotalBytesReceived;
        public long BandwidthTotalBytesReceived
        {
            get => _bandwidthTotalBytesReceived;
            set
            {
                if (value == _bandwidthTotalBytesReceived)
                    return;

                _bandwidthTotalBytesReceived = value;
                OnPropertyChanged();
            }
        }

        private long _bandwidthDiffBytesSent;
        public long BandwidthDiffBytesSent
        {
            get => _bandwidthDiffBytesSent;
            set
            {
                if (value == _bandwidthDiffBytesSent)
                    return;

                _bandwidthDiffBytesSent = value;
                OnPropertyChanged();
            }
        }

        private long _bandwidthDiffBytesReceived;
        public long BandwidthDiffBytesReceived
        {
            get => _bandwidthDiffBytesReceived;
            set
            {
                if (value == _bandwidthDiffBytesReceived)
                    return;

                _bandwidthDiffBytesReceived = value;
                OnPropertyChanged();
            }
        }

        private long _bandwithBytesReceivedSpeed;
        public long BandwidthBytesReceivedSpeed
        {
            get => _bandwithBytesReceivedSpeed;
            set
            {
                if (value == _bandwithBytesReceivedSpeed)
                    return;

                _bandwithBytesReceivedSpeed = value;
                OnPropertyChanged();
            }
        }

        private long _bandwidthBytesSentSpeed;
        public long BandwidthBytesSentSpeed
        {
            get => _bandwidthBytesSentSpeed;
            set
            {
                if (value == _bandwidthBytesSentSpeed)
                    return;

                _bandwidthBytesSentSpeed = value;
                OnPropertyChanged();
            }
        }

        private DateTime _bandwidthStartTime;
        public DateTime BandwidthStartTime
        {
            get => _bandwidthStartTime;
            set
            {
                if (value == _bandwidthStartTime)
                    return;

                _bandwidthStartTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _bandwidthMeasuredTime;
        public TimeSpan BandwidthMeasuredTime
        {
            get => _bandwidthMeasuredTime;
            set
            {
                if (value == _bandwidthMeasuredTime)
                    return;

                _bandwidthMeasuredTime = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Config
        private bool _configEnableDynamicIPAddress = true;
        public bool ConfigEnableDynamicIPAddress
        {
            get => _configEnableDynamicIPAddress;
            set
            {
                if (value == _configEnableDynamicIPAddress)
                    return;

                _configEnableDynamicIPAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableStaticIPAddress;
        public bool ConfigEnableStaticIPAddress
        {
            get => _configEnableStaticIPAddress;
            set
            {
                if (value == _configEnableStaticIPAddress)
                    return;

                ConfigEnableStaticDNS = true;

                _configEnableStaticIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _configIPAddress;
        public string ConfigIPAddress
        {
            get => _configIPAddress;
            set
            {
                if (value == _configIPAddress)
                    return;

                _configIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _configSubnetmaskOrCidr;
        public string ConfigSubnetmaskOrCidr
        {
            get => _configSubnetmaskOrCidr;
            set
            {
                if (value == _configSubnetmaskOrCidr)
                    return;

                _configSubnetmaskOrCidr = value;
                OnPropertyChanged();
            }
        }

        private string _configGateway;
        public string ConfigGateway
        {
            get => _configGateway;
            set
            {
                if (value == _configGateway)
                    return;

                _configGateway = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableDynamicDNS = true;
        public bool ConfigEnableDynamicDNS
        {
            get => _configEnableDynamicDNS;
            set
            {
                if (value == _configEnableDynamicDNS)
                    return;

                _configEnableDynamicDNS = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableStaticDNS;
        public bool ConfigEnableStaticDNS
        {
            get => _configEnableStaticDNS;
            set
            {
                if (value == _configEnableStaticDNS)
                    return;

                _configEnableStaticDNS = value;
                OnPropertyChanged();
            }
        }

        private string _configPrimaryDNSServer;
        public string ConfigPrimaryDNSServer
        {
            get => _configPrimaryDNSServer;
            set
            {
                if (value == _configPrimaryDNSServer)
                    return;

                _configPrimaryDNSServer = value;
                OnPropertyChanged();
            }
        }

        private string _configSecondaryDNSServer;
        public string ConfigSecondaryDNSServer
        {
            get => _configSecondaryDNSServer;
            set
            {
                if (value == _configSecondaryDNSServer)
                    return;

                _configSecondaryDNSServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Profiles
        public ICollectionView Profiles { get; }

        private ProfileInfo _selectedProfile = new ProfileInfo();
        public ProfileInfo SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (value == _selectedProfile)
                    return;

                if (value != null)
                {
                    ConfigEnableDynamicIPAddress = !value.NetworkInterface_EnableStaticIPAddress;
                    ConfigEnableStaticIPAddress = value.NetworkInterface_EnableStaticIPAddress;
                    ConfigIPAddress = value.NetworkInterface_IPAddress;
                    ConfigGateway = value.NetworkInterface_Gateway;
                    ConfigSubnetmaskOrCidr = value.NetworkInterface_SubnetmaskOrCidr;
                    ConfigEnableDynamicDNS = !value.NetworkInterface_EnableStaticDNS;
                    ConfigEnableStaticDNS = value.NetworkInterface_EnableStaticDNS;
                    ConfigPrimaryDNSServer = value.NetworkInterface_PrimaryDNSServer;
                    ConfigSecondaryDNSServer = value.NetworkInterface_SecondaryDNSServer;
                }

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

                StartDelayedSearch();

                OnPropertyChanged();
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (value == _isSearching)
                    return;

                _isSearching = value;
                OnPropertyChanged();
            }
        }

        private bool _canProfileWidthChange = true;
        private double _tempProfileWidth;

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get => _expandProfileView;
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.NetworkInterface_ExpandProfileView = value;

                _expandProfileView = value;

                if (_canProfileWidthChange)
                    ResizeProfile(false);

                OnPropertyChanged();
            }
        }

        private GridLength _profileWidth;
        public GridLength ProfileWidth
        {
            get => _profileWidth;
            set
            {
                if (value == _profileWidth)
                    return;

                if (!_isLoading && Math.Abs(value.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix) // Do not save the size when collapsed
                    SettingsManager.Current.NetworkInterface_ProfileWidth = value.Value;

                _profileWidth = value;

                if (_canProfileWidthChange)
                    ResizeProfile(dueToChangedSize: true);

                OnPropertyChanged();
            }
        }
        #endregion               
        #endregion

        #region Constructor, LoadSettings, OnShutdown
        public NetworkInterfaceViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            LoadNetworkInterfaces();

            InitialBandwidthChart();

            Profiles = new CollectionViewSource { Source = ProfileManager.Profiles }.View;
            Profiles.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ProfileInfo.Group)));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Group), ListSortDirection.Ascending));
            Profiles.SortDescriptions.Add(new SortDescription(nameof(ProfileInfo.Name), ListSortDirection.Ascending));
            Profiles.Filter = o =>
            {
                if (!(o is ProfileInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return info.NetworkInterface_Enabled;

                var search = Search.Trim();

                // Search by: Tag=xxx (exact match, ignore case)
                if (search.StartsWith(ProfileManager.TagIdentifier, StringComparison.OrdinalIgnoreCase))
                    return !string.IsNullOrEmpty(info.Tags) && info.NetworkInterface_Enabled && info.Tags.Replace(" ", "").Split(';').Any(str => search.Substring(ProfileManager.TagIdentifier.Length, search.Length - ProfileManager.TagIdentifier.Length).Equals(str, StringComparison.OrdinalIgnoreCase));

                // Search by: Name, IPScanner_IPRange
                return info.NetworkInterface_Enabled && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            // This will select the first entry as selected item...
            SelectedProfile = Profiles.SourceCollection.Cast<ProfileInfo>().Where(x => x.NetworkInterface_Enabled).OrderBy(x => x.Group).ThenBy(x => x.Name).FirstOrDefault();

            _searchDispatcherTimer.Interval = GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan;
            _searchDispatcherTimer.Tick += SearchDispatcherTimer_Tick;

            // Detect if network address or status changed...
            NetworkChange.NetworkAvailabilityChanged += (sender, args) => ReloadNetworkInterfacesAction();
            NetworkChange.NetworkAddressChanged += (sender, args) => ReloadNetworkInterfacesAction();

            LoadSettings();

            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        private void InitialBandwidthChart()
        {
            var dayConfig = Mappers.Xy<LvlChartsDefaultInfo>()
                .X(dayModel => (double)dayModel.DateTime.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(dayModel => dayModel.Value);

            Series = new SeriesCollection(dayConfig)
            {
                new LineSeries
                {
                    Title = "Download",
                    Values = new ChartValues<LvlChartsDefaultInfo>(),
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Upload",
                    Values = new ChartValues<LvlChartsDefaultInfo>(),
                    PointGeometry = null
                }
            };

            FormatterDate = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString("hh:mm:ss");
            FormatterSpeed = value => $"{FileSizeConverter.GetBytesReadable((long)value * 8)}it/s";
        }

        public Func<double, string> FormatterDate { get; set; }
        public Func<double, string> FormatterSpeed { get; set; }
        public SeriesCollection Series { get; set; }

        private async Task LoadNetworkInterfaces()
        {
            IsNetworkInterfaceLoading = true;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (NetworkInterfaces.Count > 0)
            {
                var info = NetworkInterfaces.FirstOrDefault(s => s.Id == SettingsManager.Current.NetworkInterface_InterfaceId);

                SelectedNetworkInterface = info ?? NetworkInterfaces[0];
            }

            IsNetworkInterfaceLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.NetworkInterface_ExpandProfileView;

            ProfileWidth = ExpandProfileView ? new GridLength(SettingsManager.Current.NetworkInterface_ProfileWidth) : new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);

            _tempProfileWidth = SettingsManager.Current.NetworkInterface_ProfileWidth;
        }
        #endregion

        #region ICommands & Actions
        public ICommand ReloadNetworkInterfacesCommand => new RelayCommand(p => ReloadNetworkInterfacesAction(), ReloadNetworkInterfaces_CanExecute);

        private bool ReloadNetworkInterfaces_CanExecute(object obj) => !IsNetworkInterfaceLoading && Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async Task ReloadNetworkInterfacesAction()
        {
            IsNetworkInterfaceLoading = true;

            await Task.Delay(2000); // Make the user happy, let him see a reload animation (and he cannot spam the reload command)

            var id = string.Empty;

            if (SelectedNetworkInterface != null)
                id = SelectedNetworkInterface.Id;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Change interface...
            SelectedNetworkInterface = string.IsNullOrEmpty(id) ? NetworkInterfaces.FirstOrDefault() : NetworkInterfaces.FirstOrDefault(x => x.Id == id);

            IsNetworkInterfaceLoading = false;
        }

        public ICommand OpenNetworkConnectionsCommand => new RelayCommand(p => OpenNetworkConnectionsAction(), OpenNetworkConnections_CanExecute);

        private bool OpenNetworkConnections_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        public async Task OpenNetworkConnectionsAction()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = "NCPA.cpl",
                    UseShellExecute = true
                };

                Process.Start(info);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand ApplyConfigurationCommand => new RelayCommand(p => ApplyConfigurationAction(), ApplyConfiguration_CanExecute);

        private bool ApplyConfiguration_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        public void ApplyConfigurationAction()
        {
            ApplyConfiguration();
        }

        public ICommand ApplyProfileConfigCommand => new RelayCommand(p => ApplyProfileProfileAction());

        private void ApplyProfileProfileAction()
        {
            ApplyProfileConfig();
        }

        public ICommand AddProfileCommand => new RelayCommand(p => AddProfileAction());

        private void AddProfileAction()
        {
            ProfileDialogManager.ShowAddProfileDialog(this, _dialogCoordinator);
        }

        public ICommand EditProfileCommand => new RelayCommand(p => EditProfileAction());

        private void EditProfileAction()
        {
            ProfileDialogManager.ShowEditProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand CopyAsProfileCommand => new RelayCommand(p => CopyAsProfileAction());

        private void CopyAsProfileAction()
        {
            ProfileDialogManager.ShowCopyAsProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand DeleteProfileCommand => new RelayCommand(p => DeleteProfileAction());

        private void DeleteProfileAction()
        {
            ProfileDialogManager.ShowDeleteProfileDialog(this, _dialogCoordinator, SelectedProfile);
        }

        public ICommand EditGroupCommand => new RelayCommand(EditGroupAction);

        private void EditGroupAction(object group)
        {
            ProfileDialogManager.ShowEditGroupDialog(this, _dialogCoordinator, group.ToString());
        }

        public ICommand FlushDNSCommand => new RelayCommand(p => FlushDNSAction(), FlushDNS_CanExecute);

        private bool FlushDNS_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async Task FlushDNSAction()
        {
            IsConfigurationRunning = true;
            IsStatusMessageDisplayed = false;

            await Models.Network.NetworkInterface.FlushDnsAsync();

            IsConfigurationRunning = false;
        }

        public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }

        public ICommand ReleaseRenewCommand => new RelayCommand(p => ReleaseRenewAction(), ReleaseRenew_CanExecute);

        private bool ReleaseRenew_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async Task ReleaseRenewAction()
        {
            IsConfigurationRunning = true;

            await Models.Network.NetworkInterface.ReleaseRenewAsync(Models.Network.NetworkInterface.IPConfigReleaseRenewMode.ReleaseRenew);

            IsConfigurationRunning = false;
        }

        public ICommand ReleaseCommand => new RelayCommand(p => ReleaseAction(), Release_CanExecute);

        private bool Release_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async Task ReleaseAction()
        {
            IsConfigurationRunning = true;

            await Models.Network.NetworkInterface.ReleaseRenewAsync(Models.Network.NetworkInterface.IPConfigReleaseRenewMode.Release);

            IsConfigurationRunning = false;
        }

        public ICommand RenewCommand => new RelayCommand(p => RenewAction(), Renew_CanExecute);

        private bool Renew_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async Task RenewAction()
        {
            IsConfigurationRunning = true;

            await Models.Network.NetworkInterface.ReleaseRenewAsync(Models.Network.NetworkInterface.IPConfigReleaseRenewMode.Renew);

            IsConfigurationRunning = false;
        }

        public ICommand AddIPv4AddressCommand => new RelayCommand(p => AddIPv4AddressAction(), AddIPv4Address_CanExecute);

        private bool AddIPv4Address_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async Task AddIPv4AddressAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddIPv4Address
            };

            var IPAddressAndSubnetmaskViewModel = new IPAddressAndSubnetmaskViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                AddIPv4Address(instance.IPAddress, instance.SubnetmaskOrCidr);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new IPAddressAndSubnetmaskDialog
            {
                DataContext = IPAddressAndSubnetmaskViewModel                
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }


        public ICommand RemoveIPv4AddressCommand => new RelayCommand(p => RemoveIPv4AddressAction(), RemoveIPv4Address_CanExecute);

        private bool RemoveIPv4Address_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private async Task RemoveIPv4AddressAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.RemoveIPv4Address
            };

            var ipAddressViewModel = new IPAddressViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                RemoveIPv4Address(instance.IPAddress);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new IPAddressDialog
            {
                DataContext = ipAddressViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private void SetConfigurationDefaults(NetworkInterfaceInfo info)
        {
            if (info.DhcpEnabled)
            {
                ConfigEnableDynamicIPAddress = true;
            }
            else
            {
                ConfigEnableStaticIPAddress = true;
                ConfigIPAddress = info.IPv4Address.FirstOrDefault()?.ToString();
                ConfigSubnetmaskOrCidr = info.Subnetmask != null ? info.Subnetmask.FirstOrDefault()?.ToString() : string.Empty;
                ConfigGateway = info.IPv4Gateway?.Any() == true ? info.IPv4Gateway.FirstOrDefault()?.ToString() : string.Empty;
            }

            if (info.DNSAutoconfigurationEnabled)
            {
                ConfigEnableDynamicDNS = true;
            }
            else
            {
                ConfigEnableStaticDNS = true;

                var dnsServers = info.DNSServer.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
                ConfigPrimaryDNSServer = dnsServers.Count > 0 ? dnsServers[0].ToString() : string.Empty;
                ConfigSecondaryDNSServer = dnsServers.Count > 1 ? dnsServers[1].ToString() : string.Empty;
            }
        }
        public async Task ApplyConfiguration()
        {
            IsConfigurationRunning = true;
            IsStatusMessageDisplayed = false;

            var subnetmask = ConfigSubnetmaskOrCidr;

            // CIDR to subnetmask
            if (ConfigEnableStaticIPAddress && subnetmask.StartsWith("/"))
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

            // If primary and secondary DNS are empty --> autoconfiguration
            if (ConfigEnableStaticDNS && string.IsNullOrEmpty(ConfigPrimaryDNSServer) && string.IsNullOrEmpty(ConfigSecondaryDNSServer))
                ConfigEnableDynamicDNS = true;

            // When primary DNS is empty, swap it with secondary (if not empty)
            if (ConfigEnableStaticDNS && string.IsNullOrEmpty(ConfigPrimaryDNSServer) && !string.IsNullOrEmpty(ConfigSecondaryDNSServer))
            {
                ConfigPrimaryDNSServer = ConfigSecondaryDNSServer;
                ConfigSecondaryDNSServer = string.Empty;
            }

            var config = new NetworkInterfaceConfig
            {
                Name = SelectedNetworkInterface.Name,
                EnableStaticIPAddress = ConfigEnableStaticIPAddress,
                IPAddress = ConfigIPAddress,
                Subnetmask = subnetmask,
                Gateway = ConfigGateway,
                EnableStaticDNS = ConfigEnableStaticDNS,
                PrimaryDNSServer = ConfigPrimaryDNSServer,
                SecondaryDNSServer = ConfigSecondaryDNSServer
            };

            try
            {
                var networkInterface = new Models.Network.NetworkInterface();

                networkInterface.UserHasCanceled += NetworkInterface_UserHasCanceled;

                await networkInterface.ConfigureNetworkInterfaceAsync(config);

                ReloadNetworkInterfacesAction();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        public async Task AddIPv4Address(string ipAddress, string subnetmaskOrCidr)
        {
            IsConfigurationRunning = true;
            IsStatusMessageDisplayed = false;

            var subnetmask = subnetmaskOrCidr;

            // CIDR to subnetmask
            if (subnetmask.StartsWith("/"))
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

            var config = new NetworkInterfaceConfig
            {
                Name = SelectedNetworkInterface.Name,
                IPAddress = ipAddress,
                Subnetmask = subnetmask
            };

            try
            {
                await Models.Network.NetworkInterface.AddIPAddressToNetworkInterfaceAsync(config);

                ReloadNetworkInterfacesAction();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        public async Task RemoveIPv4Address(string ipAddress)
        {
            IsConfigurationRunning = true;
            IsStatusMessageDisplayed = false;

            var config = new NetworkInterfaceConfig
            {
                Name = SelectedNetworkInterface.Name,
                IPAddress = ipAddress
            };

            try
            {
                await Models.Network.NetworkInterface.RemoveIPAddressFromNetworkInterfaceAsync(config);

                ReloadNetworkInterfacesAction();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        public async Task ApplyProfileConfig()
        {
            IsConfigurationRunning = true;
            IsStatusMessageDisplayed = false;

            var subnetmask = SelectedProfile.NetworkInterface_SubnetmaskOrCidr;

            // CIDR to subnetmask
            if (SelectedProfile.NetworkInterface_EnableStaticIPAddress && subnetmask.StartsWith("/"))
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetmask.TrimStart('/'))).Subnetmask;

            var enableStaticDNS = SelectedProfile.NetworkInterface_EnableStaticDNS;

            var primaryDNSServer = SelectedProfile.NetworkInterface_PrimaryDNSServer;
            var secondaryDNSServer = SelectedProfile.NetworkInterface_SecondaryDNSServer;

            // If primary and secondary DNS are empty --> autoconfiguration
            if (enableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && string.IsNullOrEmpty(secondaryDNSServer))
                enableStaticDNS = false;

            // When primary DNS is empty, swap it with secondary (if not empty)
            if (SelectedProfile.NetworkInterface_EnableStaticDNS && string.IsNullOrEmpty(primaryDNSServer) && !string.IsNullOrEmpty(secondaryDNSServer))
            {
                primaryDNSServer = secondaryDNSServer;
                secondaryDNSServer = string.Empty;
            }

            var config = new NetworkInterfaceConfig
            {
                Name = SelectedNetworkInterface.Name,
                EnableStaticIPAddress = SelectedProfile.NetworkInterface_EnableStaticIPAddress,
                IPAddress = SelectedProfile.NetworkInterface_IPAddress,
                Subnetmask = subnetmask,
                Gateway = SelectedProfile.NetworkInterface_Gateway,
                EnableStaticDNS = enableStaticDNS,
                PrimaryDNSServer = primaryDNSServer,
                SecondaryDNSServer = secondaryDNSServer
            };

            try
            {
                var networkInterface = new Models.Network.NetworkInterface();

                networkInterface.UserHasCanceled += NetworkInterface_UserHasCanceled;

                await networkInterface.ConfigureNetworkInterfaceAsync(config);

                ReloadNetworkInterfacesAction();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        private void StartDelayedSearch()
        {
            if (!IsSearching)
            {
                IsSearching = true;

                _searchDispatcherTimer.Start();
            }
            else
            {
                _searchDispatcherTimer.Stop();
                _searchDispatcherTimer.Start();
            }
        }

        private void StopDelayedSearch()
        {
            _searchDispatcherTimer.Stop();

            RefreshProfiles();

            IsSearching = false;
        }

        private void ResizeProfile(bool dueToChangedSize)
        {
            _canProfileWidthChange = false;

            if (dueToChangedSize)
            {
                ExpandProfileView = Math.Abs(ProfileWidth.Value - GlobalStaticConfiguration.Profile_WidthCollapsed) > GlobalStaticConfiguration.FloatPointFix;
            }
            else
            {
                if (ExpandProfileView)
                {
                    ProfileWidth = Math.Abs(_tempProfileWidth - GlobalStaticConfiguration.Profile_WidthCollapsed) < GlobalStaticConfiguration.FloatPointFix ? new GridLength(GlobalStaticConfiguration.Profile_DefaultWidthExpanded) : new GridLength(_tempProfileWidth);
                }
                else
                {
                    _tempProfileWidth = ProfileWidth.Value;
                    ProfileWidth = new GridLength(GlobalStaticConfiguration.Profile_WidthCollapsed);
                }
            }

            _canProfileWidthChange = true;
        }

        public void ResetBandwidthChart()
        {
            if (Series == null)
                return;

            Series[0].Values.Clear();
            Series[1].Values.Clear();

            var currentDateTime = DateTime.Now;

            for (var i = 60; i > 0; i--)
            {
                var bandwidthInfo = new LvlChartsDefaultInfo(currentDateTime.AddSeconds(-i), double.NaN);

                Series[0].Values.Add(bandwidthInfo);
                Series[1].Values.Add(bandwidthInfo);
            }
        }
        
        private bool _resetBandwidthStatisticOnNextUpdate;

        private void StartBandwidthMeter(string networkInterfaceId)
        {
            // Reset chart
            ResetBandwidthChart();

            // Reset statistic
            _resetBandwidthStatisticOnNextUpdate = true;

            _bandwidthMeter = new BandwidthMeter(networkInterfaceId);
            _bandwidthMeter.UpdateSpeed += BandwidthMeter_UpdateSpeed;
            _bandwidthMeter.Start();
        }

        private void ResumeBandwidthMeter()
        {
            if (_bandwidthMeter != null && !_bandwidthMeter.IsRunning)
            {
                ResetBandwidthChart();

                _resetBandwidthStatisticOnNextUpdate = true;

                _bandwidthMeter.Start();
            }
        }

        private void StopBandwidthMeter()
        {
            if (_bandwidthMeter != null && _bandwidthMeter.IsRunning)
                _bandwidthMeter.Stop();
        }

        public void OnViewVisible()
        {
            RefreshProfiles();

            ResumeBandwidthMeter();
        }

        public void OnViewHide()
        {
            StopBandwidthMeter();
        }

        public void RefreshProfiles()
        {
            Profiles.Refresh();
        }

        public void OnProfileDialogOpen()
        {

        }

        public void OnProfileDialogClose()
        {

        }
        #endregion

        #region Events
        private void SearchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            StopDelayedSearch();
        }

        private void BandwidthMeter_UpdateSpeed(object sender, BandwidthMeterSpeedArgs e)
        {
            // Reset statistics
            if(_resetBandwidthStatisticOnNextUpdate)
            {
                BandwidthStartTime = DateTime.Now;
                _bandwidthTotalBytesReceivedTemp = e.TotalBytesReceived;
                _bandwidthTotalBytesSentTemp = e.TotalBytesSent;

                _resetBandwidthStatisticOnNextUpdate = false;
            }

            // Measured time
            BandwidthMeasuredTime = DateTime.Now - BandwidthStartTime;

            // Current download/upload
            BandwidthTotalBytesReceived = e.TotalBytesReceived;
            BandwidthTotalBytesSent = e.TotalBytesSent;
            BandwidthBytesReceivedSpeed = e.ByteReceivedSpeed;
            BandwidthBytesSentSpeed = e.ByteSentSpeed;

            // Total download/upload
            BandwidthDiffBytesReceived = BandwidthTotalBytesReceived - _bandwidthTotalBytesReceivedTemp;
            BandwidthDiffBytesSent = BandwidthTotalBytesSent - _bandwidthTotalBytesSentTemp;

            // Add chart entry
            Series[0].Values.Add(new LvlChartsDefaultInfo(e.DateTime, e.ByteReceivedSpeed));
            Series[1].Values.Add(new LvlChartsDefaultInfo(e.DateTime, e.ByteSentSpeed));

            // Remove data older than 60 seconds
            if (Series[0].Values.Count > 59)
                Series[0].Values.RemoveAt(0);

            if (Series[1].Values.Count > 59)
                Series[1].Values.RemoveAt(0);
        }

        private void NetworkInterface_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Localization.Resources.Strings.CanceledByUserMessage;
            IsStatusMessageDisplayed = true;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
        #endregion
    }
}
