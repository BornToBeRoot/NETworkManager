using System.Collections.Generic;
using System.Windows.Input;
using System.Net;
using System.Net.NetworkInformation;
using System;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
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

namespace NETworkManager.ViewModels
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
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

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get => _displayStatusMessage;
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
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
                        SettingsManager.Current.NetworkInterface_SelectedInterfaceId = value.Id;

                    // Details
                    DetailsName = value.Name;
                    DetailsDescription = value.Description;
                    DetailsType = value.Type;
                    DetailsPhysicalAddress = value.PhysicalAddress;
                    DetailsStatus = value.Status;
                    DetailsSpeed = value.Speed;
                    DetailsIPv4Address = value.IPv4Address;
                    DetailsSubnetmask = value.Subnetmask;
                    DetailsIPv4Gateway = value.IPv4Gateway;
                    DetailsIPv4DhcpEnabled = value.DhcpEnabled;
                    DetailsIPv4DhcpServer = value.DhcpServer;
                    DetailsDhcpLeaseObtained = value.DhcpLeaseObtained;
                    DetailsDhcpLeaseExpires = value.DhcpLeaseExpires;
                    DetailsIPv6AddressLinkLocal = value.IPv6AddressLinkLocal;
                    DetailsIPv6Address = value.IPv6Address;
                    DetailsIPv6Gateway = value.IPv6Gateway;
                    DetailsDNSAutoconfigurationEnabled = value.DNSAutoconfigurationEnabled;
                    DetailsDNSSuffix = value.DNSSuffix;
                    DetailsDNSServer = value.DNSServer;

                    // Bandwidth
                    _bandwidthMeter?.Stop();

                    ResetBandwidthChart();

                    _bandwidthMeter = new BandwidthMeter(value.Id);
                    _bandwidthMeter.UpdateSpeed += BandwidthMeter_UpdateSpeed;
                    _bandwidthMeter.Start();

                    // Configuration
                    if (value.DhcpEnabled)
                    {
                        ConfigEnableDynamicIPAddress = true;
                    }
                    else
                    {
                        ConfigEnableStaticIPAddress = true;
                        ConfigIPAddress = value.IPv4Address.FirstOrDefault()?.ToString();
                        ConfigSubnetmaskOrCidr = value.Subnetmask != null ? value.Subnetmask.FirstOrDefault()?.ToString() : string.Empty;
                        ConfigGateway = value.IPv4Gateway?.Any() == true ? value.IPv4Gateway.FirstOrDefault()?.ToString() : string.Empty;
                    }

                    if (value.DNSAutoconfigurationEnabled)
                    {
                        ConfigEnableDynamicDNS = true;
                    }
                    else
                    {
                        ConfigEnableStaticDNS = true;

                        var dnsServers = value.DNSServer.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
                        ConfigPrimaryDNSServer = dnsServers.Count > 0 ? dnsServers[0].ToString() : string.Empty;
                        ConfigSecondaryDNSServer = dnsServers.Count > 1 ? dnsServers[1].ToString() : string.Empty;
                    }

                    CanConfigure = value.IsOperational;
                }

                _selectedNetworkInterface = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Details
        private string _detailsName;
        public string DetailsName
        {
            get => _detailsName;
            set
            {
                if (value == _detailsName)
                    return;

                _detailsName = value;
                OnPropertyChanged();
            }
        }

        private string _detailsDescription;
        public string DetailsDescription
        {
            get => _detailsDescription;
            set
            {
                if (value == _detailsDescription)
                    return;

                _detailsDescription = value;
                OnPropertyChanged();
            }
        }

        private string _detailsType;
        public string DetailsType
        {
            get => _detailsType;
            set
            {
                if (value == _detailsType)
                    return;

                _detailsType = value;
                OnPropertyChanged();
            }
        }

        private PhysicalAddress _detailsPhysicalAddress;
        public PhysicalAddress DetailsPhysicalAddress
        {
            get => _detailsPhysicalAddress;
            set
            {
                if (value != null && Equals(value, _detailsPhysicalAddress))
                    return;

                _detailsPhysicalAddress = value;
                OnPropertyChanged();
            }
        }

        private OperationalStatus _detailsStatus;
        public OperationalStatus DetailsStatus
        {
            get => _detailsStatus;
            set
            {
                if (value == _detailsStatus)
                    return;

                _detailsStatus = value;
                OnPropertyChanged();
            }
        }

        private long _detailsSpeed;
        public long DetailsSpeed
        {
            get => _detailsSpeed;
            set
            {
                if (value == _detailsSpeed)
                    return;

                _detailsSpeed = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv4Address;
        public IPAddress[] DetailsIPv4Address
        {
            get => _detailsIPv4Address;
            set
            {
                if (value == _detailsIPv4Address)
                    return;

                _detailsIPv4Address = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsSubnetmask;
        public IPAddress[] DetailsSubnetmask
        {
            get => _detailsSubnetmask;
            set
            {
                if (value == _detailsSubnetmask)
                    return;

                _detailsSubnetmask = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsGateway;
        public IPAddress[] DetailsIPv4Gateway
        {
            get => _detailsGateway;
            set
            {
                if (value == _detailsGateway)
                    return;

                _detailsGateway = value;
                OnPropertyChanged();
            }
        }

        private bool _detailsIPv4DhcpEnabled;
        public bool DetailsIPv4DhcpEnabled
        {
            get => _detailsIPv4DhcpEnabled;
            set
            {
                if (value == _detailsIPv4DhcpEnabled)
                    return;

                _detailsIPv4DhcpEnabled = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv4DhcpServer;
        public IPAddress[] DetailsIPv4DhcpServer
        {
            get => _detailsIPv4DhcpServer;
            set
            {
                if (value == _detailsIPv4DhcpServer)
                    return;

                _detailsIPv4DhcpServer = value;
                OnPropertyChanged();
            }
        }

        private DateTime _detailsDhcpLeaseExpires;
        public DateTime DetailsDhcpLeaseExpires
        {
            get => _detailsDhcpLeaseExpires;
            set
            {
                if (value == _detailsDhcpLeaseExpires)
                    return;

                _detailsDhcpLeaseExpires = value;
                OnPropertyChanged();
            }
        }

        private DateTime _detailsDhcpLeaseObtained;
        public DateTime DetailsDhcpLeaseObtained
        {
            get => _detailsDhcpLeaseObtained;
            set
            {
                if (value == _detailsDhcpLeaseObtained)
                    return;

                _detailsDhcpLeaseObtained = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv6AddressLinkLocal;
        public IPAddress[] DetailsIPv6AddressLinkLocal
        {
            get => _detailsIPv6AddressLinkLocal;
            set
            {
                if (value == _detailsIPv6AddressLinkLocal)
                    return;


                _detailsIPv6AddressLinkLocal = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv6Address;
        public IPAddress[] DetailsIPv6Address
        {
            get => _detailsIPv6Address;
            set
            {
                if (value == _detailsIPv6Address)
                    return;

                _detailsIPv6Address = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv6Gateway;
        public IPAddress[] DetailsIPv6Gateway
        {
            get => _detailsIPv6Gateway;
            set
            {
                if (value == _detailsIPv6Gateway)
                    return;

                _detailsIPv6Gateway = value;
                OnPropertyChanged();
            }
        }

        private bool _detailsDNSAutoconfigurationEnabled;
        public bool DetailsDNSAutoconfigurationEnabled
        {
            get => _detailsDNSAutoconfigurationEnabled;
            set
            {
                if (value == _detailsDNSAutoconfigurationEnabled)
                    return;

                _detailsDNSAutoconfigurationEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _detailsDNSSuffix;
        public string DetailsDNSSuffix
        {
            get => _detailsDNSSuffix;
            set
            {
                if (value == _detailsDNSSuffix)
                    return;

                _detailsDNSSuffix = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsDNSServer;
        public IPAddress[] DetailsDNSServer
        {
            get => _detailsDNSServer;
            set
            {
                if (value == _detailsDNSServer)
                    return;

                _detailsDNSServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Bandwidth
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

                Profiles.Refresh();

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

            LoadSettings();

            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        private void InitialBandwidthChart()
        {
            var dayConfig = Mappers.Xy<BandwidthInfo>()
                .X(dayModel => (double)dayModel.DateTime.Ticks / TimeSpan.FromHours(1).Ticks)
                .Y(dayModel => dayModel.Value);

            Series = new SeriesCollection(dayConfig)
            {
                new LineSeries
                {
                    Title = "Download",
                    Values = new ChartValues<BandwidthInfo>(),
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Upload",
                    Values = new ChartValues<BandwidthInfo>(),
                    PointGeometry = null
                }
            };

            FormatterDate = value => new DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString("hh:mm:ss");
            FormatterSpeed = value => $"{FileSizeConverter.GetBytesReadable((long)value * 8)}it/s";
        }

        public Func<double, string> FormatterDate { get; set; }
        public Func<double, string> FormatterSpeed { get; set; }
        public SeriesCollection Series { get; set; }

        private async void LoadNetworkInterfaces()
        {
            IsNetworkInterfaceLoading = true;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (NetworkInterfaces.Count > 0)
            {
                var info = NetworkInterfaces.FirstOrDefault(s => s.Id == SettingsManager.Current.NetworkInterface_SelectedInterfaceId);

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
        public ICommand ReloadNetworkInterfacesCommand
        {
            get { return new RelayCommand(p => ReloadNetworkInterfacesAction(), ReloadNetworkInterfaces_CanExecute); }
        }

        private bool ReloadNetworkInterfaces_CanExecute(object obj)
        {
            return !IsNetworkInterfaceLoading;
        }

        private async void ReloadNetworkInterfacesAction()
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

        public ICommand OpenNetworkConnectionsCommand
        {
            get { return new RelayCommand(p => OpenNetworkConnectionsAction()); }
        }

        public async void OpenNetworkConnectionsAction()
        {
            try
            {
                Process.Start("NCPA.cpl");
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }
        }

        public ICommand ApplyConfigCommand
        {
            get { return new RelayCommand(p => ApplyConfigAction()); }
        }

        public void ApplyConfigAction()
        {
            ApplyConfig();
        }

        public ICommand ApplyProfileConfigCommand
        {
            get { return new RelayCommand(p => ApplyProfileProfileAction()); }
        }

        private void ApplyProfileProfileAction()
        {
            ApplyProfileConfig();
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

        public ICommand FlushDNSCommand
        {
            get { return new RelayCommand(p => FlushDNSAction()); }
        }

        private async void FlushDNSAction()
        {
            IsConfigurationRunning = true;
            DisplayStatusMessage = false;

            await Models.Network.NetworkInterface.FlushDnsAsync();

            IsConfigurationRunning = false;
        }

        public ICommand ClearSearchCommand
        {
            get { return new RelayCommand(p => ClearSearchAction()); }
        }

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }

        public ICommand ReleaseRenewCommand
        {
            get { return new RelayCommand(p => ReleaseRenewAction()); }
        }

        private async void ReleaseRenewAction()
        {
            IsConfigurationRunning = true;

            await Models.Network.NetworkInterface.ReleaseRenewAsync(Models.Network.NetworkInterface.IPConfigReleaseRenewMode.ReleaseRenew);

            IsConfigurationRunning = false;
        }

        public ICommand ReleaseCommand
        {
            get { return new RelayCommand(p => ReleaseAction()); }
        }

        private async void ReleaseAction()
        {
            IsConfigurationRunning = true;

            await Models.Network.NetworkInterface.ReleaseRenewAsync(Models.Network.NetworkInterface.IPConfigReleaseRenewMode.Release);

            IsConfigurationRunning = false;
        }

        public ICommand RenewCommand
        {
            get { return new RelayCommand(p => RenewAction()); }
        }

        private async void RenewAction()
        {
            IsConfigurationRunning = true;

            await Models.Network.NetworkInterface.ReleaseRenewAsync(Models.Network.NetworkInterface.IPConfigReleaseRenewMode.Renew);

            IsConfigurationRunning = false;
        }

        public ICommand AddIPv4AddressCommand
        {
            get { return new RelayCommand(p => AddIPv4AddressAction()); }
        }

        private async void AddIPv4AddressAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddIPv4Address
            };

            var networkInterfaceAddIPAddressViewModel = new NetworkInterfaceAddIPAddressViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                AddIPv4Address(instance.IPAddress, instance.SubnetmaskOrCidr);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new NetworkInterfaceAddIPAddressDialog
            {
                DataContext = networkInterfaceAddIPAddressViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        #endregion

        #region Methods
        public async void ApplyConfig()
        {
            IsConfigurationRunning = true;
            DisplayStatusMessage = false;

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
                DisplayStatusMessage = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        public async void AddIPv4Address(string ipAddress, string subnetmaskOrCidr)
        {
            IsConfigurationRunning = true;
            DisplayStatusMessage = false;

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
                DisplayStatusMessage = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
        }

        public async void ApplyProfileConfig()
        {
            IsConfigurationRunning = true;
            DisplayStatusMessage = false;

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
                DisplayStatusMessage = true;
            }
            finally
            {
                IsConfigurationRunning = false;
            }
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
                var bandwidthInfo =  new BandwidthInfo(currentDateTime.AddSeconds(-i), 0);

                Series[0].Values.Add(bandwidthInfo);
                Series[1].Values.Add(bandwidthInfo);
            }
        }

        public void OnViewVisible()
        {
            // Refresh profiles
            Profiles.Refresh();

            _bandwidthMeter?.Start();
            ResetBandwidthChart();
        }

        public void OnViewHide()
        {
            _bandwidthMeter?.Stop();
        }
        #endregion

        #region Events
        private void BandwidthMeter_UpdateSpeed(object sender, BandwidthMeterSpeedArgs e)
        {
            BandwidthTotalBytesReceived = e.TotalBytesReceived;
            BandwidthTotalBytesSent = e.TotalBytesSent;
            BandwidthBytesReceivedSpeed = e.ByteReceivedSpeed;
            BandwidthBytesSentSpeed = e.ByteSentSpeed;

            Series[0].Values.Add(new BandwidthInfo(e.DateTime, e.ByteReceivedSpeed));
            Series[1].Values.Add(new BandwidthInfo(e.DateTime, e.ByteSentSpeed));

            // Remove data older than 60 seconds
            if (Series[0].Values.Count >= 60)
                Series[0].Values.RemoveAt(0);

            if (Series[1].Values.Count >= 60)
                Series[1].Values.RemoveAt(0);
        }

        private void NetworkInterface_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Resources.Localization.Strings.CanceledByUserMessage;
            DisplayStatusMessage = true;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
        #endregion
    }
}
