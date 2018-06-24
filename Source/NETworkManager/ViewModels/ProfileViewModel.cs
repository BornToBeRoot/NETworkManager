using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private ProfileInfo _profileInfo;

        #region General
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                    return;

                _name = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value == _host)
                    return;

                _host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private CredentialInfo _credential = null;
        public CredentialInfo Credential
        {
            get { return _credential; }
            set
            {
                if (value == _credential)
                    return;

                _credential = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        ICollectionView _credentials;
        public ICollectionView Credentials
        {
            get { return _credentials; }
        }

        private string _group;
        public string Group
        {
            get { return _group; }
            set
            {
                if (value == _group)
                    return;

                _group = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        ICollectionView _groups;
        public ICollectionView Groups
        {
            get { return _groups; }
        }

        private string _tags;
        public string Tags
        {
            get { return _tags; }
            set
            {
                if (value == _tags)
                    return;

                _tags = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _noTabEnabled;
        public bool NoTabEnabled
        {
            get { return _noTabEnabled; }
            set
            {
                if (value == _noTabEnabled)
                    return;

                _noTabEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _showUnlockCredentialsHint;
        public bool ShowUnlockCredentialsHint
        {
            get { return _showUnlockCredentialsHint; }
            set
            {
                if (value == _showUnlockCredentialsHint)
                    return;

                _showUnlockCredentialsHint = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Network Interface
        private bool _networkInterface_Enabled;
        public bool NetworkInterface_Enabled
        {
            get { return _networkInterface_Enabled; }
            set
            {
                if (value == _networkInterface_Enabled)
                    return;

                _networkInterface_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _networkInterface_EnableDynamicIPAddress = true;
        public bool NetworkInterface_EnableDynamicIPAddress
        {
            get { return _networkInterface_EnableDynamicIPAddress; }
            set
            {
                if (value == _networkInterface_EnableDynamicIPAddress)
                    return;

                _networkInterface_EnableDynamicIPAddress = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _networkInterface_EnableStaticIPAddress;
        public bool NetworkInterface_EnableStaticIPAddress
        {
            get { return _networkInterface_EnableStaticIPAddress; }
            set
            {
                if (value == _networkInterface_EnableStaticIPAddress)
                    return;

                if (value)
                    NetworkInterface_EnableStaticDNS = true;
                else
                    NetworkInterface_EnableStaticDNS = true;

                _networkInterface_EnableStaticIPAddress = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _networkInterface_IPAddress;
        public string NetworkInterface_IPAddress
        {
            get { return _networkInterface_IPAddress; }
            set
            {
                if (value == _networkInterface_IPAddress)
                    return;

                _networkInterface_IPAddress = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _networkInterface_SubnetmaskOrCidr;
        public string NetworkInterface_SubnetmaskOrCidr
        {
            get { return _networkInterface_SubnetmaskOrCidr; }
            set
            {
                if (value == _networkInterface_SubnetmaskOrCidr)
                    return;

                _networkInterface_SubnetmaskOrCidr = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _networkInterface_Gateway;
        public string NetworkInterface_Gateway
        {
            get { return _networkInterface_Gateway; }
            set
            {
                if (value == _networkInterface_Gateway)
                    return;

                _networkInterface_Gateway = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _networkInterface_EnableDynamicDNS = true;
        public bool NetworkInterface_EnableDynamicDNS
        {
            get { return _networkInterface_EnableDynamicDNS; }
            set
            {
                if (value == _networkInterface_EnableDynamicDNS)
                    return;

                _networkInterface_EnableDynamicDNS = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _networkInterface_EnableStaticDNS;
        public bool NetworkInterface_EnableStaticDNS
        {
            get { return _networkInterface_EnableStaticDNS; }
            set
            {
                if (value == _networkInterface_EnableStaticDNS)
                    return;

                _networkInterface_EnableStaticDNS = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _networkInterface_PrimaryDNSServer;
        public string NetworkInterface_PrimaryDNSServer
        {
            get { return _networkInterface_PrimaryDNSServer; }
            set
            {
                if (value == _networkInterface_PrimaryDNSServer)
                    return;

                _networkInterface_PrimaryDNSServer = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _networkInterface_SecondaryDNSServer;
        public string NetworkInterface_SecondaryDNSServer
        {
            get { return _networkInterface_SecondaryDNSServer; }
            set
            {
                if (value == _networkInterface_SecondaryDNSServer)
                    return;

                _networkInterface_SecondaryDNSServer = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region IP Scanner
        private bool _ipScanner_Enabled;
        public bool IPScanner_Enabled
        {
            get { return _ipScanner_Enabled; }
            set
            {
                if (value == _ipScanner_Enabled)
                    return;

                _ipScanner_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _ipScanner_InheritHost;
        public bool IPScanner_InheritHost
        {
            get { return _ipScanner_InheritHost; }
            set
            {
                if (value == _ipScanner_InheritHost)
                    return;

                _ipScanner_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _ipScanner_IPRange;
        public string IPScanner_IPRange
        {
            get { return _ipScanner_IPRange; }
            set
            {
                if (value == _ipScanner_IPRange)
                    return;

                _ipScanner_IPRange = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region Port Scanner
        private bool _portScanner_Enabled;
        public bool PortScanner_Enabled
        {
            get { return _portScanner_Enabled; }
            set
            {
                if (value == _portScanner_Enabled)
                    return;

                _portScanner_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _portScanner_InheritHost;
        public bool PortScanner_InheritHost
        {
            get { return _portScanner_InheritHost; }
            set
            {
                if (value == _portScanner_InheritHost)
                    return;

                _portScanner_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _portScanner_Host;
        public string PortScanner_Host
        {
            get { return _portScanner_Host; }
            set
            {
                if (value == _portScanner_Host)
                    return;

                _portScanner_Host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _portScanner_Ports;
        public string PortScanner_Ports
        {
            get { return _portScanner_Ports; }
            set
            {
                if (value == _portScanner_Ports)
                    return;

                _portScanner_Ports = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region Ping
        private bool _ping_Enabled;
        public bool Ping_Enabled
        {
            get { return _ping_Enabled; }
            set
            {
                if (value == _ping_Enabled)
                    return;

                _ping_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _ping_InheritHost;
        public bool Ping_InheritHost
        {
            get { return _ping_InheritHost; }
            set
            {
                if (value == _ping_InheritHost)
                    return;

                _ping_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _ping_Host;
        public string Ping_Host
        {
            get { return _ping_Host; }
            set
            {
                if (value == _ping_Host)
                    return;

                _ping_Host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region Traceroute
        private bool _traceroute_Enabled;
        public bool Traceroute_Enabled
        {
            get { return _traceroute_Enabled; }
            set
            {
                if (value == _traceroute_Enabled)
                    return;

                _traceroute_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _traceroute_InheritHost;
        public bool Traceroute_InheritHost
        {
            get { return _traceroute_InheritHost; }
            set
            {
                if (value == _traceroute_InheritHost)
                    return;

                _traceroute_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _traceroute_Host;
        public string Traceroute_Host
        {
            get { return _traceroute_Host; }
            set
            {
                if (value == _traceroute_Host)
                    return;

                _traceroute_Host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region RemoteDesktop
        private bool _remoteDesktop_Enabled;
        public bool RemoteDesktop_Enabled
        {
            get { return _remoteDesktop_Enabled; }
            set
            {
                if (value == _remoteDesktop_Enabled)
                    return;

                _remoteDesktop_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_InheritHost;
        public bool RemoteDesktop_InheritHost
        {
            get { return _remoteDesktop_InheritHost; }
            set
            {
                if (value == _remoteDesktop_InheritHost)
                    return;

                _remoteDesktop_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _remoteDesktop_Host;
        public string RemoteDesktop_Host
        {
            get { return _remoteDesktop_Host; }
            set
            {
                if (value == _remoteDesktop_Host)
                    return;

                _remoteDesktop_Host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region PuTTY 
        private bool _puTTY_Enabled;
        public bool PuTTY_Enabled
        {
            get { return _puTTY_Enabled; }
            set
            {
                if (value == _puTTY_Enabled)
                    return;

                _puTTY_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _puTTY_InheritHost;
        public bool PuTTY_InheritHost
        {
            get { return _puTTY_InheritHost; }
            set
            {
                if (value == _puTTY_InheritHost)
                    return;

                _puTTY_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _puTTY_UseSSH; // Default is SSH
        public bool PuTTY_UseSSH
        {
            get { return _puTTY_UseSSH; }
            set
            {
                if (value == _puTTY_UseSSH)
                    return;

                if (value)
                {
                    PuTTY_Port = SettingsManager.Current.PuTTY_SSHPort;
                    PuTTY_ConnectionMode = ConnectionMode.SSH;
                }

                _puTTY_UseSSH = value;
                OnPropertyChanged();
            }
        }

        private bool _puTTY_UseTelnet;
        public bool PuTTY_UseTelnet
        {
            get { return _puTTY_UseTelnet; }
            set
            {
                if (value == _puTTY_UseTelnet)
                    return;

                if (value)
                {
                    PuTTY_Port = SettingsManager.Current.PuTTY_TelnetPort;
                    PuTTY_ConnectionMode = ConnectionMode.Telnet;
                }

                _puTTY_UseTelnet = value;
                OnPropertyChanged();
            }
        }

        private bool _puTTY_UseSerial;
        public bool PuTTY_UseSerial
        {
            get { return _puTTY_UseSerial; }
            set
            {
                if (value == _puTTY_UseSerial)
                    return;

                if (value)
                {
                    PuTTY_Baud = SettingsManager.Current.PuTTY_BaudRate;
                    PuTTY_ConnectionMode = ConnectionMode.Serial;
                }

                _puTTY_UseSerial = value;
                OnPropertyChanged();
            }
        }

        private bool _puTTY_UseRlogin;
        public bool PuTTY_UseRlogin
        {
            get { return _puTTY_UseRlogin; }
            set
            {
                if (value == _puTTY_UseRlogin)
                    return;

                if (value)
                {
                    PuTTY_Port = SettingsManager.Current.PuTTY_RloginPort;
                    PuTTY_ConnectionMode = ConnectionMode.Rlogin;
                }

                _puTTY_UseRlogin = value;
                OnPropertyChanged();
            }
        }

        private bool _puTTY_UseRAW;
        public bool PuTTY_UseRAW
        {
            get { return _puTTY_UseRAW; }
            set
            {
                if (value == _puTTY_UseRAW)
                    return;

                if (value)
                {
                    PuTTY_Port = 0;
                    PuTTY_ConnectionMode = ConnectionMode.RAW;
                }

                _puTTY_UseRAW = value;
                OnPropertyChanged();
            }
        }

        private string _puTTY_Host;
        public string PuTTY_Host
        {
            get { return _puTTY_Host; }
            set
            {
                if (value == _puTTY_Host)
                    return;

                _puTTY_Host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _puTTY_SerialLine;
        public string PuTTY_SerialLine
        {
            get { return _puTTY_SerialLine; }
            set
            {
                if (value == _puTTY_SerialLine)
                    return;

                _puTTY_SerialLine = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private int _puTTY_Port;
        public int PuTTY_Port
        {
            get { return _puTTY_Port; }
            set
            {
                if (value == _puTTY_Port)
                    return;

                _puTTY_Port = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private int _puTTY_Baud;
        public int PuTTY_Baud
        {
            get { return _puTTY_Baud; }
            set
            {
                if (value == _puTTY_Baud)
                    return;

                _puTTY_Baud = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _puTTY__Username;
        public string PuTTY_Username
        {
            get { return _puTTY__Username; }
            set
            {
                if (value == _puTTY__Username)
                    return;

                _puTTY__Username = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _puTTY_Profile;
        public string PuTTY_Profile
        {
            get { return _puTTY_Profile; }
            set
            {
                if (value == _puTTY_Profile)
                    return;

                _puTTY_Profile = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _puTTY_AdditionalCommandLine;
        public string PuTTY_AdditionalCommandLine
        {
            get { return _puTTY_AdditionalCommandLine; }
            set
            {
                if (value == _puTTY_AdditionalCommandLine)
                    return;

                _puTTY_AdditionalCommandLine = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private ConnectionMode _puTTY_ConnectionMode;
        public ConnectionMode PuTTY_ConnectionMode
        {
            get { return _puTTY_ConnectionMode; }
            set
            {
                if (value == _puTTY_ConnectionMode)
                    return;

                _puTTY_ConnectionMode = value;

                if (!_isLoading)
                    Validate();
            }
        }
        #endregion

        #region Wake on LAN
        private bool _wakeOnLAN_Enabled;
        public bool WakeOnLAN_Enabled
        {
            get { return _wakeOnLAN_Enabled; }
            set
            {
                if (value == _wakeOnLAN_Enabled)
                    return;

                _wakeOnLAN_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _wakeOnLAN_MACAddress;
        public string WakeOnLAN_MACAddress
        {
            get { return _wakeOnLAN_MACAddress; }
            set
            {
                if (value == _wakeOnLAN_MACAddress)
                    return;

                _wakeOnLAN_MACAddress = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _wakeOnLAN_Broadcast;
        public string WakeOnLAN_Broadcast
        {
            get { return _wakeOnLAN_Broadcast; }
            set
            {
                if (value == _wakeOnLAN_Broadcast)
                    return;

                _wakeOnLAN_Broadcast = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private int _wakeOnLAN_Port;
        public int WakeOnLAN_Port
        {
            get { return _wakeOnLAN_Port; }
            set
            {
                if (value == _wakeOnLAN_Port)
                    return;

                _wakeOnLAN_Port = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        public ProfileViewModel(Action<ProfileViewModel> saveCommand, Action<ProfileViewModel> cancelHandler, List<string> groups, ProfileInfo profileInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _profileInfo = profileInfo ?? new ProfileInfo();

            Name = _profileInfo.Name;
            Host = _profileInfo.Host;

            if (CredentialManager.Loaded)
            {
                _credentials = new CollectionViewSource { Source = CredentialManager.CredentialInfoList }.View;
            }
            else
            {
                ShowUnlockCredentialsHint = true;

                if (_profileInfo.CredentialID == null)
                    _credentials = new CollectionViewSource { Source = new List<CredentialInfo>() }.View;
                else
                    _credentials = new CollectionViewSource { Source = new List<CredentialInfo>() { new CredentialInfo((int)_profileInfo.CredentialID) } }.View;
            }

            Credential = Credentials.SourceCollection.Cast<CredentialInfo>().FirstOrDefault(x => x.ID == _profileInfo.CredentialID);

            Group = string.IsNullOrEmpty(_profileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : LocalizationManager.GetStringByKey("String_Default")) : _profileInfo.Group;
            Tags = _profileInfo.Tags;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            // Network Interface
            NetworkInterface_Enabled = _profileInfo.NetworkInterface_Enabled;
            NetworkInterface_EnableDynamicIPAddress = !_profileInfo.NetworkInterface_EnableStaticIPAddress;
            NetworkInterface_EnableStaticIPAddress = _profileInfo.NetworkInterface_EnableStaticIPAddress;
            NetworkInterface_IPAddress = _profileInfo.NetworkInterface_IPAddress;
            NetworkInterface_Gateway = _profileInfo.NetworkInterface_Gateway;
            NetworkInterface_SubnetmaskOrCidr = _profileInfo.NetworkInterface_SubnetmaskOrCidr;
            NetworkInterface_EnableDynamicDNS = !_profileInfo.NetworkInterface_EnableStaticDNS;
            NetworkInterface_EnableStaticDNS = _profileInfo.NetworkInterface_EnableStaticDNS;
            NetworkInterface_PrimaryDNSServer = _profileInfo.NetworkInterface_PrimaryDNSServer;
            NetworkInterface_SecondaryDNSServer = _profileInfo.NetworkInterface_SecondaryDNSServer;

            // IP Scanner
            IPScanner_Enabled = _profileInfo.IPScanner_Enabled;
            IPScanner_InheritHost = _profileInfo.IPScanner_InheritHost;
            IPScanner_IPRange = _profileInfo.IPScanner_IPRange;

            // Port Scanner
            PortScanner_Enabled = _profileInfo.PortScanner_Enabled;
            PortScanner_InheritHost = _profileInfo.PortScanner_InheritHost;
            PortScanner_Host = _profileInfo.PortScanner_Host;
            PortScanner_Ports = _profileInfo.PortScanner_Ports;

            // Ping
            Ping_Enabled = _profileInfo.Ping_Enabled;
            Ping_InheritHost = _profileInfo.Ping_InheritHost;
            Ping_Host = _profileInfo.Ping_Host;

            // Traceroute
            Traceroute_Enabled = _profileInfo.Traceroute_Enabled;
            Traceroute_InheritHost = _profileInfo.Traceroute_InheritHost;
            Traceroute_Host = _profileInfo.Traceroute_Host;

            // Remote Desktop
            RemoteDesktop_Enabled = _profileInfo.RemoteDesktop_Enabled;
            RemoteDesktop_InheritHost = _profileInfo.RemoteDesktop_InheritHost;
            RemoteDesktop_Host = _profileInfo.RemoteDesktop_Host;

            // PuTTY
            PuTTY_Enabled = _profileInfo.PuTTY_Enabled;

            switch (_profileInfo.PuTTY_ConnectionMode)
            {
                // SSH is default
                case ConnectionMode.SSH:
                    PuTTY_UseSSH = true;
                    break;
                case ConnectionMode.Telnet:
                    PuTTY_UseTelnet = true;
                    break;
                case ConnectionMode.Serial:
                    PuTTY_UseSerial = true;
                    break;
                case ConnectionMode.Rlogin:
                    PuTTY_UseRlogin = true;
                    break;
                case ConnectionMode.RAW:
                    PuTTY_UseRAW = true;
                    break;
            }

            PuTTY_InheritHost = _profileInfo.PuTTY_InheritHost;

            if (_profileInfo.PuTTY_ConnectionMode == ConnectionMode.Serial)
            {
                PuTTY_SerialLine = _profileInfo.PuTTY_HostOrSerialLine;
                PuTTY_Baud = _profileInfo.PuTTY_PortOrBaud;
            }
            else
            {
                PuTTY_Host = _profileInfo.PuTTY_HostOrSerialLine;
                PuTTY_Port = _profileInfo.PuTTY_PortOrBaud == 0 ? SettingsManager.Current.PuTTY_SSHPort : _profileInfo.PuTTY_PortOrBaud; // Default SSH port
            }

            PuTTY_Username = _profileInfo.PuTTY_Username;
            PuTTY_Profile = _profileInfo.PuTTY_Profile;
            PuTTY_AdditionalCommandLine = _profileInfo.PuTTY_AdditionalCommandLine;

            WakeOnLAN_Enabled = _profileInfo.WakeOnLAN_Enabled;
            WakeOnLAN_MACAddress = _profileInfo.WakeOnLAN_MACAddress;
            WakeOnLAN_Broadcast = _profileInfo.WakeOnLAN_Broadcast;
            WakeOnLAN_Port = _profileInfo.WakeOnLAN_Port == 0 ? SettingsManager.Current.WakeOnLAN_DefaultPort : _profileInfo.WakeOnLAN_Port;

            Validate();

            _isLoading = false;
        }

        private void Validate()
        {
            // Note
            NoTabEnabled = (NetworkInterface_Enabled || IPScanner_Enabled || PortScanner_Enabled || Ping_Enabled || Traceroute_Enabled || RemoteDesktop_Enabled || PuTTY_Enabled || WakeOnLAN_Enabled);
        }

        #region ICommands & Actions
        private readonly ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get { return _saveCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        public ICommand UnselectCredentialCommand
        {
            get { return new RelayCommand(p => UnselectCredentialAction()); }
        }

        private void UnselectCredentialAction()
        {
            Credential = null;
        }
        #endregion
    }
}