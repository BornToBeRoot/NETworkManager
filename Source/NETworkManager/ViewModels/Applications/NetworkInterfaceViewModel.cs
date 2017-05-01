using System.Collections.Generic;
using System.Windows.Input;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MahApps.Metro.Controls.Dialogs;
using System.Collections;
using NETworkManager.Settings;
using NETworkManager.Settings.Templates;
using NETworkManager.Model.Network;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels.Applications
{
    class NetworkInterfaceViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        private bool _isLoading = true;

        public bool IsAdmin
        {
            get { return Configuration.Current.IsAdmin; }
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
                        Properties.Settings.Default.NetworkInterface_SelectedInterfaceID = value.Id;

                        SettingsManager.SettingsChanged = true;
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
        private bool _configUseDynamicIPv4Address = true;
        public bool ConfigUseDynamicIPv4Address
        {
            get { return _configUseDynamicIPv4Address; }
            set
            {
                if (value == _configUseDynamicIPv4Address)
                    return;

                _configUseDynamicIPv4Address = value;
                OnPropertyChanged();
            }
        }

        private bool _configUseStaticIPv4Address;
        public bool ConfigUseStaticIPv4Address
        {
            get { return _configUseStaticIPv4Address; }
            set
            {
                if (value == _configUseStaticIPv4Address)
                    return;

                ConfigUseStaticIPv4DNSServer = true;

                _configUseStaticIPv4Address = value;
                OnPropertyChanged();
            }
        }

        private string _configIPv4Address;
        public string ConfigIPv4Address
        {
            get { return _configIPv4Address; }
            set
            {
                if (value == _configIPv4Address)
                    return;

                _configIPv4Address = value;
                OnPropertyChanged();
            }
        }

        private string _configSubnetmask;
        public string ConfigSubnetmask
        {
            get { return _configSubnetmask; }
            set
            {
                if (value == _configSubnetmask)
                    return;

                _configSubnetmask = value;
                OnPropertyChanged();
            }
        }

        private string _configIPv4Gateway;
        public string ConfigIPv4Gateway
        {
            get { return _configIPv4Gateway; }
            set
            {
                if (value == _configIPv4Gateway)
                    return;

                _configIPv4Gateway = value;
                OnPropertyChanged();
            }
        }

        private bool _configUseDynamicIPv4DNSServer = true;
        public bool ConfigUseDynamicIPv4DNSServer
        {
            get { return _configUseDynamicIPv4DNSServer; }
            set
            {
                if (value == _configUseDynamicIPv4DNSServer)
                    return;

                _configUseDynamicIPv4DNSServer = value;
                OnPropertyChanged();
            }
        }

        private bool _configUseStaticIPv4DNSServer;
        public bool ConfigUseStaticIPv4DNSServer
        {
            get { return _configUseStaticIPv4DNSServer; }
            set
            {
                if (value == _configUseStaticIPv4DNSServer)
                    return;

                _configUseStaticIPv4DNSServer = value;
                OnPropertyChanged();
            }
        }

        private string _configIPv4PrimaryDNSServer;
        public string ConfigIPv4PrimaryDNSServer
        {
            get { return _configIPv4PrimaryDNSServer; }
            set
            {
                if (value == _configIPv4PrimaryDNSServer)
                    return;

                _configIPv4PrimaryDNSServer = value;
                OnPropertyChanged();
            }
        }

        private string _configIPv4SecondaryDNSServer;
        public string ConfigIPv4SecondaryDNSServer
        {
            get { return _configIPv4SecondaryDNSServer; }
            set
            {
                if (value == _configIPv4SecondaryDNSServer)
                    return;

                _configIPv4SecondaryDNSServer = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Templates
        private bool _configTemplatesChanged;
        public bool ConfigTemplatesChanged
        {
            get { return _configTemplatesChanged; }
            set
            {
                if (value == _configTemplatesChanged)
                    return;

                _configTemplatesChanged = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<NetworkInterfaceTemplate> _configTemplates = new ObservableCollection<NetworkInterfaceTemplate>();

        public ObservableCollection<NetworkInterfaceTemplate> ConfigTemplates
        {
            get { return _configTemplates; }
            set
            {
                if (value == _configTemplates)
                    return;

                _configTemplates = value;
            }
        }

        private NetworkInterfaceTemplate _selectedConfigTemplate;
        public NetworkInterfaceTemplate SelectedConfigTemplate
        {
            get { return _selectedConfigTemplate; }
            set
            {
                if (value == _selectedConfigTemplate)
                    return;

                if (value != null)
                {
                    if (value.UseStaticIPv4Address)
                    {
                        ConfigUseStaticIPv4Address = true;
                        ConfigIPv4Address = value.IPv4Address;
                        ConfigIPv4Gateway = value.IPv4Gateway;
                        ConfigSubnetmask = value.Subnetmask;
                    }
                    else
                    {
                        ConfigUseDynamicIPv4Address = true;
                    }

                    if (value.UseStaticIPv4DNSServer)
                    {
                        ConfigUseStaticIPv4DNSServer = true;
                        ConfigIPv4PrimaryDNSServer = value.IPv4PrimaryDNSServer;
                        ConfigIPv4SecondaryDNSServer = value.IPv4SecondaryDNSServer;
                    }
                    else
                    {
                        ConfigUseDynamicIPv4DNSServer = true;
                    }
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

            // Load templates 
            LoadTemplates();

            _isLoading = false;
        }

        private void LoadTemplates()
        {
            foreach (NetworkInterfaceTemplate template in SettingsManager.GetNetworkInterfaceConfigTemplates())
            {
                ConfigTemplates.Add(template);
            }

            // Add collection changed event to detect changed (add/remove)
            ConfigTemplates.CollectionChanged += ConfigTemplates_CollectionChanged;
        }

        public void OnShutdown()
        {
            if (ConfigTemplatesChanged)
                SaveTemplates();
        }
        #endregion

        #region ICommands
        public ICommand ReloadNetworkInterfacesCommand
        {
            get { return new RelayCommand(p => ReloadNetworkInterfacesAction()); }
        }

        public ICommand ApplyNetworkInterfaceConfigCommand
        {
            get { return new RelayCommand(p => ApplyNetworkInterfaceConfigAction()); }
        }

        public ICommand AddTemplateCommand
        {
            get { return new RelayCommand(p => AddTemplateAction()); }
        }

        public ICommand UnselectTemplateCommand
        {
            get { return new RelayCommand(p => UnselectTemplateAction()); }
        }
        public ICommand DeleteSelectedConfigTemplatesCommand
        {
            get { return new RelayCommand(p => DeleteSelectedConfigTemplatesAction()); }
        }
        #endregion

        #region Methods
        private async void LoadNetworkInterfaces()
        {
            IsNetworkInterfaceLoading = true;
            await Task.Delay(1000); // Make the user happy, let him see a reload animation

            NetworkInterfaces = await Model.Network.NetworkInterface.GetNetworkInterfacesAsync();

            // Get the last selected interface, if it is still available on this machine...
            if (NetworkInterfaces.Count > 0)
            {
                NetworkInterfaceInfo info = NetworkInterfaces.Where(s => s.Id == Properties.Settings.Default.NetworkInterface_SelectedInterfaceID).FirstOrDefault();

                if (info != null)
                    SelectedNetworkInterface = info;
                else
                    SelectedNetworkInterface = NetworkInterfaces[0];
            }

            IsNetworkInterfaceLoading = false;
        }

        private async void ReloadNetworkInterfacesAction()
        {
            IsNetworkInterfaceLoading = true;
            await Task.Delay(2500); // Make the user happy, let him see a reload animation

            string id = string.Empty;

            if (SelectedNetworkInterface != null)
                id = SelectedNetworkInterface.Id;

            NetworkInterfaces = await Model.Network.NetworkInterface.GetNetworkInterfacesAsync();

            SelectedNetworkInterface = NetworkInterfaces.Where(x => x.Id == id).FirstOrDefault();

            IsNetworkInterfaceLoading = false;
        }

        ProgressDialogController progressDialogController;

        public async void ApplyNetworkInterfaceConfigAction()
        {
            progressDialogController = await dialogCoordinator.ShowProgressAsync(this, Application.Current.Resources["String_ProgessHeader_ConfigureNetworkInterface"] as string, string.Empty);
            progressDialogController.SetIndeterminate();

            try
            {
                NetworkInterfaceConfig config = new NetworkInterfaceConfig
                {
                    Id = SelectedNetworkInterface.Id,
                    EnableStaticIPAddress = ConfigUseStaticIPv4Address,
                    IPAddress = ConfigIPv4Address,
                    Subnetmask = ConfigSubnetmask,
                    Gateway = ConfigIPv4Gateway,
                    EnableStaticDns = ConfigUseStaticIPv4DNSServer,
                    PrimaryDnsServer = ConfigIPv4PrimaryDNSServer,
                    SecondaryDnsServer = ConfigIPv4SecondaryDNSServer
                };

                Model.Network.NetworkInterface networkInterface = new Model.Network.NetworkInterface();
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

        private void NetworkInterface_ConfigureProgressChanged(object sender, Model.Common.ProgressChangedArgs e)
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

        private async void AddTemplateAction()
        {
            MetroDialogSettings dialogSettings = new MetroDialogSettings();

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_Add"] as string;
            dialogSettings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            string name = await dialogCoordinator.ShowInputAsync(this, Application.Current.Resources["String_AddTemplate"] as string, Application.Current.Resources["String_EnterNameForTemplate"] as string, dialogSettings);

            if (string.IsNullOrEmpty(name))
                return;

            NetworkInterfaceTemplate template = new NetworkInterfaceTemplate
            {
                Name = name,
                UseStaticIPv4Address = ConfigUseStaticIPv4Address,
                IPv4Address = ConfigIPv4Address,
                IPv4Gateway = ConfigIPv4Gateway,
                Subnetmask = ConfigSubnetmask,
                UseStaticIPv4DNSServer = ConfigUseStaticIPv4DNSServer,
                IPv4PrimaryDNSServer = ConfigIPv4PrimaryDNSServer,
                IPv4SecondaryDNSServer = ConfigIPv4SecondaryDNSServer
            };

            ConfigTemplates.Add(template);
        }

        private void UnselectTemplateAction()
        {
            SelectedConfigTemplate = null;
        }

        private async void DeleteSelectedConfigTemplatesAction()
        {
            MetroDialogSettings dialogSettings = new MetroDialogSettings();

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            dialogSettings.AffirmativeButtonText = Application.Current.Resources["String_Button_Delete"] as string;
            dialogSettings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

            dialogSettings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            MessageDialogResult result = await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_AreYouSure"] as string, Application.Current.Resources["String_DeleteTemplatesMessage"] as string, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (result == MessageDialogResult.Negative)
                return;

            List<NetworkInterfaceTemplate> list = new List<NetworkInterfaceTemplate>();

            foreach (NetworkInterfaceTemplate tmpInfo in SelectedConfigTemplates)
                list.Add(tmpInfo);

            foreach (NetworkInterfaceTemplate info in list)
                ConfigTemplates.Remove(info);
        }

        public void SaveTemplates()
        {
            SettingsManager.SaveNetworkInterfaceConfigTemplates(new List<NetworkInterfaceTemplate>(ConfigTemplates));

            ConfigTemplatesChanged = false;
        }
        #endregion

        #region Events
        private void ConfigTemplates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConfigTemplatesChanged = true;
        }
        #endregion
    }
}
