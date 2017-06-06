using System.Collections.Generic;
using System.Windows.Input;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using System.Collections;
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

        MetroDialogSettings dialogSettings = new MetroDialogSettings();

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
                    {
                        SettingsManager.Current.NetworkInterface_SelectedInterfaceId = value.Id;

                        //SettingsManager.Current.SettingsChanged = true;
                    }

                    DetailsName = value.Name;
                    DetailsDescription = value.Description;
                    DetailsType = value.Type;
                    DetailsPhysicalAddress = value.PhysicalAddress;
                    DetailsStatus = value.Status;
                    DetailsSpeed = value.Speed;
                    DetailsIPv4Address = value.IPv4Address;
                    DetailsSubnetmask = value.Subnetmask;
                    DetailsIPv4Gateway = value.IPv4Gateway;
                    DetailsIsIPv4DhcpEnabled = value.IsDhcpEnabled;
                    DetailsIsIPv4DhcpServer = value.DhcpServer;
                    DetailsDhcpLeaseObtained = value.DhcpLeaseObtained;
                    DetailsDhcpLeaseExpires = value.DhcpLeaseExpires;
                    DetailsIPv6AddressLinkLocal = value.IPv6AddressLinkLocal;
                    DetailsIPv6Address = value.IPv6Address;
                    DetailsIPv6Gateway = value.IPv6Gateway;
                    DetailsDnsSuffix = value.DnsSuffix;
                    DetailsDnsServer = value.DnsServer;
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

        private string _detailsStatus;
        public string DetailsStatus
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

        private bool _detailsIsIPv4DhcpEnabled;
        public bool DetailsIsIPv4DhcpEnabled
        {
            get { return _detailsIsIPv4DhcpEnabled; }
            set
            {
                if (value == _detailsIsIPv4DhcpEnabled)
                    return;

                _detailsIsIPv4DhcpEnabled = value;
                OnPropertyChanged();
            }
        }

        private IPAddress[] _detailsIsIPv4DhcpServer;
        public IPAddress[] DetailsIsIPv4DhcpServer
        {
            get { return _detailsIsIPv4DhcpServer; }
            set
            {
                if (value == _detailsIsIPv4DhcpServer)
                    return;

                _detailsIsIPv4DhcpServer = value;
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

        #region Templates
        ICollectionView _networkInterfaceConfigTemplates;
        public ICollectionView NetworkInterfaceConfigTemplates
        {
            get { return _networkInterfaceConfigTemplates; }
        }


        private TemplateNetworkInterfaceConfig _selectedConfigTemplate = new TemplateNetworkInterfaceConfig();
        public TemplateNetworkInterfaceConfig SelectedConfigTemplate
        {
            get { return _selectedConfigTemplate; }
            set
            {
                if (value == _selectedConfigTemplate)
                    return;

                if (value != null)
                {
                    ConfigEnableDynamicIPAddress = !value.EnableStaticIPAddress;
                    ConfigEnableStaticIPAddress = value.EnableStaticIPAddress;
                    ConfigIPAddress = value.IPAddress;
                    ConfigGateway = value.Gateway;
                    ConfigSubnetmaskOrCidr = value.Subnetmask;
                    ConfigEnableDynamicDns = !value.EnableStaticDns;
                    ConfigPrimaryDnsServer = value.PrimaryDnsServer;
                    ConfigSecondaryDnsServer = value.SecondaryDnsServer;
                }

                _selectedConfigTemplate = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedConfigTemplates = new ArrayList();
        public IList SelectedConfigTemplates
        {
            get { return _selectedConfigTemplates; }
            set
            {
                _selectedConfigTemplates = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion

        #region Constructor, LoadTemplates, OnShutdown
        public NetworkInterfaceViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            // Load network interfaces
            LoadNetworkInterfaces();

            _networkInterfaceConfigTemplates = CollectionViewSource.GetDefaultView(TemplateManager.NetworkInterfaceConfigTemplates);

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
        #endregion

        #region ICommands & Actions
        public ICommand ReloadNetworkInterfacesCommand
        {
            get { return new RelayCommand(p => ReloadNetworkInterfacesAction()); }
        }

        private async void ReloadNetworkInterfacesAction()
        {
            IsNetworkInterfaceLoading = true;
            await Task.Delay(2500); // Make the user happy, let him see a reload animation

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
            progressDialogController = await dialogCoordinator.ShowProgressAsync(this, Application.Current.Resources["String_ProgessHeader_ConfigureNetworkInterface"] as string, string.Empty);
            progressDialogController.SetIndeterminate();

            string configSubnetmask = ConfigSubnetmaskOrCidr;

            if (ConfigSubnetmaskOrCidr.StartsWith("/"))
                configSubnetmask = Subnetmask.GetFromCidr(int.Parse(ConfigSubnetmaskOrCidr.TrimStart('/'))).Subnetmask;

            try
            {
                NetworkInterfaceConfig config = new NetworkInterfaceConfig
                {
                    Id = SelectedNetworkInterface.Id,
                    EnableStaticIPAddress = ConfigEnableStaticIPAddress,
                    IPAddress = ConfigIPAddress,
                    Subnetmask = configSubnetmask,
                    Gateway = ConfigGateway,
                    EnableStaticDns = ConfigEnableStaticDns,
                    PrimaryDnsServer = ConfigPrimaryDnsServer,
                    SecondaryDnsServer = ConfigSecondaryDnsServer
                };

                Models.Network.NetworkInterface networkInterface = new Models.Network.NetworkInterface();
                networkInterface.ConfigureProgressChanged += NetworkInterface_ConfigureProgressChanged;
                await networkInterface.ConfigureNetworkInterfaceAsync(config);
            }
            catch (Exception ex)
            {

                dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_OK"] as string;

                await dialogCoordinator.ShowMessageAsync(this, "Error", ex.Message, MessageDialogStyle.Affirmative, dialogSettings);
            }
            finally
            {
                await progressDialogController.CloseAsync();
            }

            ReloadNetworkInterfacesAction();
        }

        public ICommand AddTemplateCommand
        {
            get { return new RelayCommand(p => AddTemplateAction()); }
        }

        private async void AddTemplateAction()
        {
            MetroDialogSettings dialogSettings = new MetroDialogSettings()
            {
                CustomResourceDictionary = new ResourceDictionary
                {
                    Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
                },

                AffirmativeButtonText = Application.Current.Resources["String_Button_Add"] as string,
                NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string
            };

            string name = await dialogCoordinator.ShowInputAsync(this, Application.Current.Resources["String_AddTemplate"] as string, Application.Current.Resources["String_EnterNameForTemplate"] as string, dialogSettings);

            if (string.IsNullOrEmpty(name))
                return;

            string configSubnetmask = ConfigSubnetmaskOrCidr;

            if (ConfigEnableStaticIPAddress && ConfigSubnetmaskOrCidr.StartsWith("/"))
                configSubnetmask = Subnetmask.GetFromCidr(int.Parse(ConfigSubnetmaskOrCidr.TrimStart('/'))).Subnetmask;

            TemplateNetworkInterfaceConfig template = new TemplateNetworkInterfaceConfig
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

            TemplateManager.NetworkInterfaceConfigTemplates.Add(template);
        }

        public ICommand UnselectTemplateCommand
        {
            get { return new RelayCommand(p => UnselectTemplateAction()); }
        }

        private void UnselectTemplateAction()
        {
            SelectedConfigTemplate = null;
        }

        public ICommand DeleteSelectedConfigTemplatesCommand
        {
            get { return new RelayCommand(p => DeleteSelectedConfigTemplatesAction()); }
        }

        private async void DeleteSelectedConfigTemplatesAction()
        {
            MetroDialogSettings dialogSettings = new MetroDialogSettings()
            {
                CustomResourceDictionary = new ResourceDictionary
                {
                    Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
                },

                AffirmativeButtonText = Application.Current.Resources["String_Button_Delete"] as string,
                NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string,

                DefaultButtonFocus = MessageDialogResult.Affirmative
            };

            MessageDialogResult result = await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_AreYouSure"] as string, Application.Current.Resources["String_DeleteTemplatesMessage"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (result == MessageDialogResult.Negative)
                return;

            List<TemplateNetworkInterfaceConfig> list = new List<TemplateNetworkInterfaceConfig>();

            foreach (TemplateNetworkInterfaceConfig template in SelectedConfigTemplates)
            {
                list.Add(template);
            }

            foreach (TemplateNetworkInterfaceConfig info in list)
            {
                TemplateManager.NetworkInterfaceConfigTemplates.Remove(info);
            }
        }
        #endregion

        #region Events
        private void NetworkInterface_ConfigureProgressChanged(object sender, ProgressChangedArgs e)
        {
            switch (e.Value)
            {
                case 1:
                    progressDialogController.SetMessage(Application.Current.Resources["String_Progress_SetStaticIPAddress"] as string);
                    break;
                case 2:
                    progressDialogController.SetMessage(Application.Current.Resources["String_Progress_SetDynamicIPAddress"] as string);
                    break;
                case 3:
                    progressDialogController.SetMessage(Application.Current.Resources["String_Progress_SetStaticDNSServer"] as string);
                    break;
                case 4:
                    progressDialogController.SetMessage(Application.Current.Resources["String_Progesss_SetDynamicDNSServer"] as string);
                    break;
                case 5:
                    progressDialogController.SetMessage(Application.Current.Resources["String_Progress_RenewDHCPLease"] as string);
                    break;
                case 6:
                    progressDialogController.SetMessage(Application.Current.Resources["String_Progress_FixGatewayAfterDHCPEnabled"] as string);
                    break;
            }
        }
        #endregion
    }
}
