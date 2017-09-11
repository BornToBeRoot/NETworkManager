using System.Collections.Generic;
using System.Windows.Input;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Models.Network;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;

namespace NETworkManager.ViewModels.Applications
{
    public class NetworkInterfaceViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        ProgressDialogController progressDialogController;

        private bool _isLoading = true;

        public bool IsAdmin
        {
            get { return ConfigurationManager.Current.IsAdmin; }
        }

        private bool _isNetworkInteraceLoading;
        public bool IsNetworkInterfaceLoading
        {
            get { return _isNetworkInteraceLoading; }
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
            get { return _canConfigure; }
            set
            {
                if (value == _canConfigure)
                    return;

                _canConfigure = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get { return _displayStatusMessage; }
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
            get { return _statusMessage; }
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
            get { return _networkInterfaces; }
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
            get { return _selectedNetworkInterface; }
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
                    DetailsDnsAutoconfigurationEnabled = value.DnsAutoconfigurationEnabled;
                    DetailsDnsSuffix = value.DnsSuffix;
                    DetailsDnsServer = value.DnsServer;

                    // Configuration
                    if(value.DhcpEnabled)
                    {
                        ConfigEnableDynamicIPAddress = true;
                    }
                    else
                    {
                        ConfigEnableStaticIPAddress = true;
                        ConfigIPAddress = value.IPv4Address.FirstOrDefault().ToString();
                        ConfigSubnetmaskOrCidr = value.Subnetmask.FirstOrDefault().ToString();
                        ConfigGateway = value.IPv4Gateway.FirstOrDefault().ToString();
                    }

                    if(value.DnsAutoconfigurationEnabled)
                    {
                        ConfigEnableDynamicDns = true;
                    }
                    else
                    {
                        ConfigEnableStaticDns = true;

                        List<IPAddress> dnsServers = value.DnsServer.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
                        ConfigPrimaryDnsServer = dnsServers.Count > 0 ? dnsServers[0].ToString() : string.Empty ;
                        ConfigSecondaryDnsServer = dnsServers.Count > 1 ? dnsServers[1].ToString() : string.Empty;
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
            get { return _detailsName; }
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
            get { return _detailsDescription; }
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
            get { return _detailsType; }
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
            get { return _detailsPhysicalAddress; }
            set
            {
                if (value == _detailsPhysicalAddress)
                    return;

                _detailsPhysicalAddress = value;
                OnPropertyChanged();
            }
        }

        private OperationalStatus _detailsStatus;
        public OperationalStatus DetailsStatus
        {
            get { return _detailsStatus; }
            set
            {
                if (value == _detailsStatus)
                    return;

                _detailsStatus = value;
                OnPropertyChanged();
            }
        }

        private long detailsSpeed;
        public long DetailsSpeed
        {
            get { return detailsSpeed; }
            set
            {
                if (value == detailsSpeed)
                    return;

                detailsSpeed = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIPv4Address;
        public IPAddress[] DetailsIPv4Address
        {
            get { return _detailsIPv4Address; }
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
            get { return _detailsSubnetmask; }
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
            get { return _detailsGateway; }
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
            get { return _detailsIPv4DhcpEnabled; }
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
            get { return _detailsIPv4DhcpServer; }
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
            get { return _detailsDhcpLeaseExpires; }
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
            get { return _detailsDhcpLeaseObtained; }
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
            get { return _detailsIPv6AddressLinkLocal; }
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
            get { return _detailsIPv6Address; }
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
            get { return _detailsIPv6Gateway; }
            set
            {
                if (value == _detailsIPv6Gateway)
                    return;

                _detailsIPv6Gateway = value;
                OnPropertyChanged();
            }
        }

        private bool _detailsDnsAutoconfigurationEnabled;
        public bool DetailsDnsAutoconfigurationEnabled
        {
            get { return _detailsDnsAutoconfigurationEnabled; }
            set
            {
                if (value == _detailsDnsAutoconfigurationEnabled)
                    return;

                _detailsDnsAutoconfigurationEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _detailsDnsSuffix;
        public string DetailsDnsSuffix
        {
            get { return _detailsDnsSuffix; }
            set
            {
                if (value == _detailsDnsSuffix)
                    return;

                _detailsDnsSuffix = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsDnsServer;
        public IPAddress[] DetailsDnsServer
        {
            get { return _detailsDnsServer; }
            set
            {
                if (value == _detailsDnsServer)
                    return;

                _detailsDnsServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Config
        private bool _configEnableDynamicIPAddress = true;
        public bool ConfigEnableDynamicIPAddress
        {
            get { return _configEnableDynamicIPAddress; }
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
            get { return _configEnableStaticIPAddress; }
            set
            {
                if (value == _configEnableStaticIPAddress)
                    return;

                ConfigEnableStaticDns = true;

                _configEnableStaticIPAddress = value;
                OnPropertyChanged();
            }
        }

        private string _configIPAddress;
        public string ConfigIPAddress
        {
            get { return _configIPAddress; }
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
            get { return _configSubnetmaskOrCidr; }
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
            get { return _configGateway; }
            set
            {
                if (value == _configGateway)
                    return;

                _configGateway = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableDynamicDns = true;
        public bool ConfigEnableDynamicDns
        {
            get { return _configEnableDynamicDns; }
            set
            {
                if (value == _configEnableDynamicDns)
                    return;

                _configEnableDynamicDns = value;
                OnPropertyChanged();
            }
        }

        private bool _configEnableStaticDns;
        public bool ConfigEnableStaticDns
        {
            get { return _configEnableStaticDns; }
            set
            {
                if (value == _configEnableStaticDns)
                    return;

                _configEnableStaticDns = value;
                OnPropertyChanged();
            }
        }

        private string _configPrimaryDnsServer;
        public string ConfigPrimaryDnsServer
        {
            get { return _configPrimaryDnsServer; }
            set
            {
                if (value == _configPrimaryDnsServer)
                    return;

                _configPrimaryDnsServer = value;
                OnPropertyChanged();
            }
        }

        private string _configSecondaryDnsServer;
        public string ConfigSecondaryDnsServer
        {
            get { return _configSecondaryDnsServer; }
            set
            {
                if (value == _configSecondaryDnsServer)
                    return;

                _configSecondaryDnsServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Profiles
        ICollectionView _networkInterfaceProfiles;
        public ICollectionView NetworkInterfaceProfiles
        {
            get { return _networkInterfaceProfiles; }
        }

        private NetworkInterfaceProfileInfo _selectedProfile = new NetworkInterfaceProfileInfo();
        public NetworkInterfaceProfileInfo SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (value == _selectedProfile)
                    return;

                if (value != null)
                {
                    ConfigEnableDynamicIPAddress = !value.EnableStaticIPAddress;
                    ConfigEnableStaticIPAddress = value.EnableStaticIPAddress;
                    ConfigIPAddress = value.IPAddress;
                    ConfigGateway = value.Gateway;
                    ConfigSubnetmaskOrCidr = value.Subnetmask;
                    ConfigEnableDynamicDns = !value.EnableStaticDns;
                    ConfigEnableStaticDns = value.EnableStaticDns;
                    ConfigPrimaryDnsServer = value.PrimaryDnsServer;
                    ConfigSecondaryDnsServer = value.SecondaryDnsServer;
                }

                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        private bool _expandProfileView;
        public bool ExpandProfileView
        {
            get { return _expandProfileView; }
            set
            {
                if (value == _expandProfileView)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.NetworkInterface_ExpandProfileView = value;

                _expandProfileView = value;
                OnPropertyChanged();
            }
        }
        #endregion               
        #endregion

        #region Constructor, LoadTemplates, LoadSettings, OnShutdown
        public NetworkInterfaceViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Load network interfaces
            LoadNetworkInterfaces();

            // Load profiles
            NetworkInterfaceProfileManager.Load();
            _networkInterfaceProfiles = CollectionViewSource.GetDefaultView(NetworkInterfaceProfileManager.Profiles);

            LoadSettings();

            _isLoading = false;
        }

        private async void LoadNetworkInterfaces()
        {
            IsNetworkInterfaceLoading = true;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (NetworkInterfaces.Count > 0)
            {
                NetworkInterfaceInfo info = NetworkInterfaces.Where(s => s.Id == SettingsManager.Current.NetworkInterface_SelectedInterfaceId).FirstOrDefault();

                if (info != null)
                    SelectedNetworkInterface = info;
                else
                    SelectedNetworkInterface = NetworkInterfaces[0];
            }

            IsNetworkInterfaceLoading = false;
        }

        private void LoadSettings()
        {
            ExpandProfileView = SettingsManager.Current.NetworkInterface_ExpandProfileView;
        }

        public void OnShutdown()
        {
            if (NetworkInterfaceProfileManager.ProfilesChanged)
                NetworkInterfaceProfileManager.Save();
        }
        #endregion

        #region ICommands & Actions
        public ICommand ReloadNetworkInterfacesCommand
        {
            get { return new RelayCommand(p => ReloadNetworkInterfacesAction()); }
        }

        private async void ReloadNetworkInterfacesAction()
        {
            IsNetworkInterfaceLoading = true;
            await Task.Delay(2000); // Make the user happy, let him see a reload animation

            string id = string.Empty;

            if (SelectedNetworkInterface != null)
                id = SelectedNetworkInterface.Id;

            NetworkInterfaces = await Models.Network.NetworkInterface.GetNetworkInterfacesAsync();

            SelectedNetworkInterface = NetworkInterfaces.Where(x => x.Id == id).FirstOrDefault();

            IsNetworkInterfaceLoading = false;
        }

        public ICommand ApplyNetworkInterfaceConfigCommand
        {
            get { return new RelayCommand(p => ApplyNetworkInterfaceConfigAction()); }
        }

        public async void ApplyNetworkInterfaceConfigAction()
        {
            DisplayStatusMessage = false;

            progressDialogController = await dialogCoordinator.ShowProgressAsync(this, Application.Current.Resources["String_ProgessHeader_ConfigureNetworkInterface"] as string, string.Empty);
            progressDialogController.SetIndeterminate();

            string configSubnetmask = ConfigSubnetmaskOrCidr;

            if (ConfigEnableStaticIPAddress && ConfigSubnetmaskOrCidr.StartsWith("/"))
                configSubnetmask = Subnetmask.GetFromCidr(int.Parse(ConfigSubnetmaskOrCidr.TrimStart('/'))).Subnetmask;

            NetworkInterfaceConfig config = new NetworkInterfaceConfig
            {
                Name = SelectedNetworkInterface.Name,
                EnableStaticIPAddress = ConfigEnableStaticIPAddress,
                IPAddress = ConfigIPAddress,
                Subnetmask = configSubnetmask,
                Gateway = ConfigGateway,
                EnableStaticDns = ConfigEnableStaticDns,
                PrimaryDnsServer = ConfigPrimaryDnsServer,
                SecondaryDnsServer = ConfigSecondaryDnsServer
            };

            try
            {
                Models.Network.NetworkInterface networkInterface = new Models.Network.NetworkInterface();

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
                await progressDialogController.CloseAsync();
            }
        }

        private void NetworkInterface_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
            DisplayStatusMessage = true;
        }

        public ICommand AddProfileCommand
        {
            get { return new RelayCommand(p => AddProfileAction()); }
        }

        private async void AddProfileAction()
        {
            MetroDialogSettings settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Application.Current.Resources["String_Button_Add"] as string;
            settings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            string name = await dialogCoordinator.ShowInputAsync(this, Application.Current.Resources["String_Header_AddProfile"] as string, Application.Current.Resources["String_EnterNameForProfile"] as string, settings);

            if (string.IsNullOrEmpty(name))
                return;

            string configSubnetmask = ConfigSubnetmaskOrCidr;

            if (ConfigEnableStaticIPAddress && ConfigSubnetmaskOrCidr.StartsWith("/"))
                configSubnetmask = Subnetmask.GetFromCidr(int.Parse(ConfigSubnetmaskOrCidr.TrimStart('/'))).Subnetmask;

            NetworkInterfaceProfileInfo profile = new NetworkInterfaceProfileInfo
            {
                Name = name,
                EnableStaticIPAddress = ConfigEnableStaticIPAddress,
                IPAddress = ConfigIPAddress,
                Gateway = ConfigGateway,
                Subnetmask = configSubnetmask,
                EnableStaticDns = ConfigEnableStaticDns,
                PrimaryDnsServer = ConfigPrimaryDnsServer,
                SecondaryDnsServer = ConfigSecondaryDnsServer
            };

            NetworkInterfaceProfileManager.AddProfile(profile);
        }

        public ICommand UnselectProfileCommand
        {
            get { return new RelayCommand(p => UnselectProfileAction()); }
        }

        private void UnselectProfileAction()
        {
            SelectedProfile = null;
        }

        public ICommand DeleteProfileCommand
        {
            get { return new RelayCommand(p => DeleteProfileAction()); }
        }

        private async void DeleteProfileAction()
        {
            MetroDialogSettings settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Application.Current.Resources["String_Button_Delete"] as string;
            settings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            if (MessageDialogResult.Negative == await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, Application.Current.Resources["String_DeleteProfileMessage"] as string, MessageDialogStyle.AffirmativeAndNegative, settings))
                return;

            NetworkInterfaceProfileManager.RemoveProfile(SelectedProfile);
        }
        #endregion
    }
}
