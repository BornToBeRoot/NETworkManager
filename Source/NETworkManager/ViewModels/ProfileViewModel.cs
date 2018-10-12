using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using static NETworkManager.Models.PuTTY.PuTTY;
// ReSharper disable InconsistentNaming

namespace NETworkManager.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        #region General
        private string _name;
        public string Name
        {
            get => _name;
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
            get => _host;
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

        private Guid _credentialID;
        public Guid CredentialID
        {
            get => _credentialID;
            set
            {
                if (value == _credentialID)
                    return;

                _credentialID = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        public ICollectionView Credentials { get; }

        private string _group;
        public string Group
        {
            get => _group;
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

        public ICollectionView Groups { get; }

        private string _tags;
        public string Tags
        {
            get => _tags;
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

        private bool _isTabEnabled;
        public bool IsTabEnabled
        {
            get => _isTabEnabled;
            set
            {
                if (value == _isTabEnabled)
                    return;

                _isTabEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _showUnlockCredentialsHint;
        public bool ShowUnlockCredentialsHint
        {
            get => _showUnlockCredentialsHint;
            set
            {
                if (value == _showUnlockCredentialsHint)
                    return;

                _showUnlockCredentialsHint = value;
                OnPropertyChanged();
            }
        }

        private bool _isEdited;
        public bool IsEdited
        {
            get => _isEdited;
            set
            {
                if (value == _isEdited)
                    return;

                _isEdited = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Network Interface
        private bool _networkInterface_Enabled;
        public bool NetworkInterface_Enabled
        {
            get => _networkInterface_Enabled;
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
            get => _networkInterface_EnableDynamicIPAddress;
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
            get => _networkInterface_EnableStaticIPAddress;
            set
            {
                if (value == _networkInterface_EnableStaticIPAddress)
                    return;

                if (value)
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
            get => _networkInterface_IPAddress;
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
            get => _networkInterface_SubnetmaskOrCidr;
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
            get => _networkInterface_Gateway;
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
            get => _networkInterface_EnableDynamicDNS;
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
            get => _networkInterface_EnableStaticDNS;
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
            get => _networkInterface_PrimaryDNSServer;
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
            get => _networkInterface_SecondaryDNSServer;
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
            get => _ipScanner_Enabled;
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
            get => _ipScanner_InheritHost;
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
            get => _ipScanner_IPRange;
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
            get => _portScanner_Enabled;
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
            get => _portScanner_InheritHost;
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
            get => _portScanner_Host;
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
            get => _portScanner_Ports;
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
            get => _ping_Enabled;
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
            get => _ping_InheritHost;
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
            get => _ping_Host;
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
            get => _traceroute_Enabled;
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
            get => _traceroute_InheritHost;
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
            get => _traceroute_Host;
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

        #region DNS Lookup
        private bool _dnsLookup_Enabled;
        public bool DNSLookup_Enabled
        {
            get => _dnsLookup_Enabled;
            set
            {
                if (value == _dnsLookup_Enabled)
                    return;

                _dnsLookup_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _dnsLookup_InheritHost;
        public bool DNSLookup_InheritHost
        {
            get => _dnsLookup_InheritHost;
            set
            {
                if (value == _dnsLookup_InheritHost)
                    return;

                _dnsLookup_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _dnsLookup_Host;
        public string DNSLookup_Host
        {
            get => _dnsLookup_Host;
            set
            {
                if (value == _dnsLookup_Host)
                    return;

                _dnsLookup_Host = value;

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
            get => _remoteDesktop_Enabled;
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
            get => _remoteDesktop_InheritHost;
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
            get => _remoteDesktop_Host;
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
            get => _puTTY_Enabled;
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
            get => _puTTY_InheritHost;
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
            get => _puTTY_UseSSH;
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
            get => _puTTY_UseTelnet;
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
            get => _puTTY_UseSerial;
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
            get => _puTTY_UseRlogin;
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
            get => _puTTY_UseRAW;
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
            get => _puTTY_Host;
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
            get => _puTTY_SerialLine;
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
            get => _puTTY_Port;
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
            get => _puTTY_Baud;
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
            get => _puTTY__Username;
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
            get => _puTTY_Profile;
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
            get => _puTTY_AdditionalCommandLine;
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
            get => _puTTY_ConnectionMode;
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
            get => _wakeOnLAN_Enabled;
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
            get => _wakeOnLAN_MACAddress;
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
            get => _wakeOnLAN_Broadcast;
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
            get => _wakeOnLAN_Port;
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

        #region HTTP Headers
        private bool _httpHeaders_Enabled;
        public bool HTTPHeaders_Enabled
        {
            get => _httpHeaders_Enabled;
            set
            {
                if (value == _httpHeaders_Enabled)
                    return;

                _httpHeaders_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _httpHeaders_Website;
        public string HTTPHeaders_Website
        {
            get => _httpHeaders_Website;
            set
            {
                if (value == _httpHeaders_Website)
                    return;

                _httpHeaders_Website = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion

        #region Whois
        private bool _whois_Enabled;
        public bool Whois_Enabled
        {
            get => _whois_Enabled;
            set
            {
                if (value == _whois_Enabled)
                    return;

                _whois_Enabled = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private bool _whois_InheritHost;
        public bool Whois_InheritHost
        {
            get => _whois_InheritHost;
            set
            {
                if (value == _whois_InheritHost)
                    return;

                _whois_InheritHost = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }

        private string _whois_Host;
        public string Whois_Host
        {
            get => _whois_Host;
            set
            {
                if (value == _whois_Host)
                    return;

                _whois_Host = value;

                if (!_isLoading)
                    Validate();

                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        public ProfileViewModel(Action<ProfileViewModel> saveCommand, Action<ProfileViewModel> cancelHandler, IReadOnlyCollection<string> groups, bool isEdited = false, ProfileInfo profileInfo = null)
        {
            _isLoading = true;

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            IsEdited = isEdited;

            var profileInfo2 = profileInfo ?? new ProfileInfo();

            Name = profileInfo2.Name;
            Host = profileInfo2.Host;

            if (CredentialManager.IsLoaded)
            {
                Credentials = CollectionViewSource.GetDefaultView(CredentialManager.CredentialInfoList);
            }
            else
            {
                ShowUnlockCredentialsHint = true;

                Credentials = profileInfo2.CredentialID == Guid.Empty ? new CollectionViewSource { Source = new List<CredentialInfo>() }.View : new CollectionViewSource { Source = new List<CredentialInfo>() { new CredentialInfo(profileInfo2.CredentialID) } }.View;
            }

            CredentialID = profileInfo2.CredentialID;

            Group = string.IsNullOrEmpty(profileInfo2.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : Resources.Localization.Strings.Default) : profileInfo2.Group;
            Tags = profileInfo2.Tags;

            Groups = CollectionViewSource.GetDefaultView(groups);
            Groups.SortDescriptions.Add(new SortDescription());

            // Network Interface
            NetworkInterface_Enabled = profileInfo2.NetworkInterface_Enabled;
            NetworkInterface_EnableDynamicIPAddress = !profileInfo2.NetworkInterface_EnableStaticIPAddress;
            NetworkInterface_EnableStaticIPAddress = profileInfo2.NetworkInterface_EnableStaticIPAddress;
            NetworkInterface_IPAddress = profileInfo2.NetworkInterface_IPAddress;
            NetworkInterface_Gateway = profileInfo2.NetworkInterface_Gateway;
            NetworkInterface_SubnetmaskOrCidr = profileInfo2.NetworkInterface_SubnetmaskOrCidr;
            NetworkInterface_EnableDynamicDNS = !profileInfo2.NetworkInterface_EnableStaticDNS;
            NetworkInterface_EnableStaticDNS = profileInfo2.NetworkInterface_EnableStaticDNS;
            NetworkInterface_PrimaryDNSServer = profileInfo2.NetworkInterface_PrimaryDNSServer;
            NetworkInterface_SecondaryDNSServer = profileInfo2.NetworkInterface_SecondaryDNSServer;

            // IP Scanner
            IPScanner_Enabled = profileInfo2.IPScanner_Enabled;
            IPScanner_InheritHost = profileInfo2.IPScanner_InheritHost;
            IPScanner_IPRange = profileInfo2.IPScanner_IPRange;

            // Port Scanner
            PortScanner_Enabled = profileInfo2.PortScanner_Enabled;
            PortScanner_InheritHost = profileInfo2.PortScanner_InheritHost;
            PortScanner_Host = profileInfo2.PortScanner_Host;
            PortScanner_Ports = profileInfo2.PortScanner_Ports;

            // Ping
            Ping_Enabled = profileInfo2.Ping_Enabled;
            Ping_InheritHost = profileInfo2.Ping_InheritHost;
            Ping_Host = profileInfo2.Ping_Host;

            // Traceroute
            Traceroute_Enabled = profileInfo2.Traceroute_Enabled;
            Traceroute_InheritHost = profileInfo2.Traceroute_InheritHost;
            Traceroute_Host = profileInfo2.Traceroute_Host;

            // DNS Lookup
            DNSLookup_Enabled = profileInfo2.DNSLookup_Enabled;
            DNSLookup_InheritHost = profileInfo2.DNSLookup_InheritHost;
            DNSLookup_Host = profileInfo2.DNSLookup_Host;

            // Remote Desktop
            RemoteDesktop_Enabled = profileInfo2.RemoteDesktop_Enabled;
            RemoteDesktop_InheritHost = profileInfo2.RemoteDesktop_InheritHost;
            RemoteDesktop_Host = profileInfo2.RemoteDesktop_Host;

            // PuTTY
            PuTTY_Enabled = profileInfo2.PuTTY_Enabled;

            switch (profileInfo2.PuTTY_ConnectionMode)
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

            PuTTY_InheritHost = profileInfo2.PuTTY_InheritHost;

            if (profileInfo2.PuTTY_ConnectionMode == ConnectionMode.Serial)
            {
                PuTTY_SerialLine = profileInfo2.PuTTY_HostOrSerialLine;
                PuTTY_Baud = profileInfo2.PuTTY_PortOrBaud;
            }
            else
            {
                PuTTY_Host = profileInfo2.PuTTY_HostOrSerialLine;
                PuTTY_Port = profileInfo2.PuTTY_PortOrBaud == 0 ? SettingsManager.Current.PuTTY_SSHPort : profileInfo2.PuTTY_PortOrBaud; // Default SSH port
            }

            PuTTY_Username = profileInfo2.PuTTY_Username;
            PuTTY_Profile = profileInfo2.PuTTY_Profile;
            PuTTY_AdditionalCommandLine = profileInfo2.PuTTY_AdditionalCommandLine;

            // Wake on LAN
            WakeOnLAN_Enabled = profileInfo2.WakeOnLAN_Enabled;
            WakeOnLAN_MACAddress = profileInfo2.WakeOnLAN_MACAddress;
            WakeOnLAN_Broadcast = profileInfo2.WakeOnLAN_Broadcast;
            WakeOnLAN_Port = profileInfo2.WakeOnLAN_Port == 0 ? SettingsManager.Current.WakeOnLAN_DefaultPort : profileInfo2.WakeOnLAN_Port;

            // HTTP Headers
            HTTPHeaders_Enabled = profileInfo2.HTTPHeaders_Enabled;
            HTTPHeaders_Website = profileInfo2.HTTPHeaders_Website;

            // Whois
            Whois_Enabled = profileInfo2.Whois_Enabled;
            Whois_InheritHost = profileInfo2.Whois_InheritHost;
            Whois_Host = profileInfo2.Whois_Host;

            Validate();

            _isLoading = false;
        }

        private void Validate()
        {
            // Note
            IsTabEnabled = NetworkInterface_Enabled || IPScanner_Enabled || PortScanner_Enabled || Ping_Enabled || Traceroute_Enabled || DNSLookup_Enabled || RemoteDesktop_Enabled || PuTTY_Enabled || WakeOnLAN_Enabled || HTTPHeaders_Enabled || Whois_Enabled;
        }

        #region ICommands & Actions
        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand UnselectCredentialCommand
        {
            get { return new RelayCommand(p => UnselectCredentialAction()); }
        }

        private void UnselectCredentialAction()
        {
            CredentialID = Guid.Empty;
        }
        #endregion
    }
}