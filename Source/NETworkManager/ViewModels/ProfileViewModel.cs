using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.PowerShell;
using NETworkManager.Enum;
using static NETworkManager.Models.PuTTY.PuTTY;
using NETworkManager.Models.RemoteDesktop;
// ReSharper disable InconsistentNaming

namespace NETworkManager.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;
        public ICollectionView ProfileViews { get; }
        
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

                // Reset, if string has changed
                if (!IsResolveHostnameRunning)
                    ShowCouldNotResolveHostnameWarning = false;

                _host = value;
                OnPropertyChanged();
            }
        }

        private bool _isResolveHostnameRunning;
        public bool IsResolveHostnameRunning
        {
            get => _isResolveHostnameRunning;
            set
            {
                if (value == _isResolveHostnameRunning)
                    return;

                _isResolveHostnameRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _showCouldNotResolveHostnameWarning;
        public bool ShowCouldNotResolveHostnameWarning
        {
            get => _showCouldNotResolveHostnameWarning;
            set
            {
                if (value == _showCouldNotResolveHostnameWarning)
                    return;

                _showCouldNotResolveHostnameWarning = value;
                OnPropertyChanged();
            }
        }

        private string _group;
        public string Group
        {
            get => _group;
            set
            {
                if (value == _group)
                    return;

                _group = value;
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
                OnPropertyChanged();
            }
        }

        private string _ipScanner_HostOrIPRange;
        public string IPScanner_HostOrIPRange
        {
            get => _ipScanner_HostOrIPRange;
            set
            {
                if (value == _ipScanner_HostOrIPRange)
                    return;

                _ipScanner_HostOrIPRange = value;
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
                OnPropertyChanged();
            }
        }

        #endregion

        #region Ping Monitor
        private bool _pingMonitor_Enabled;
        public bool PingMonitor_Enabled
        {
            get => _pingMonitor_Enabled;
            set
            {
                if (value == _pingMonitor_Enabled)
                    return;

                _pingMonitor_Enabled = value;

                OnPropertyChanged();
            }
        }

        private bool _pingMonitor_InheritHost;
        public bool PingMonitor_InheritHost
        {
            get => _pingMonitor_InheritHost;
            set
            {
                if (value == _pingMonitor_InheritHost)
                    return;

                _pingMonitor_InheritHost = value;
                OnPropertyChanged();
            }
        }

        private string _pingMonitor_Host;
        public string PingMonitor_Host
        {
            get => _pingMonitor_Host;
            set
            {
                if (value == _pingMonitor_Host)
                    return;

                _pingMonitor_Host = value;
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
                OnPropertyChanged();
            }
        }
        #endregion

        #region Remote Desktop
        private bool _remoteDesktop_Enabled;
        public bool RemoteDesktop_Enabled
        {
            get => _remoteDesktop_Enabled;
            set
            {
                if (value == _remoteDesktop_Enabled)
                    return;

                _remoteDesktop_Enabled = value;

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
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideDisplay;
        public bool RemoteDesktop_OverrideDisplay
        {
            get => _remoteDesktop_OverrideDisplay;
            set
            {
                if (value == _remoteDesktop_OverrideDisplay)
                    return;

                _remoteDesktop_OverrideDisplay = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_AdjustScreenAutomatically;
        public bool RemoteDesktop_AdjustScreenAutomatically
        {
            get => _remoteDesktop_AdjustScreenAutomatically;
            set
            {
                if (value == _remoteDesktop_AdjustScreenAutomatically)
                    return;

                _remoteDesktop_AdjustScreenAutomatically = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_UseCurrentViewSize;
        public bool RemoteDesktop_UseCurrentViewSize
        {
            get => _remoteDesktop_UseCurrentViewSize;
            set
            {
                if (value == _remoteDesktop_UseCurrentViewSize)
                    return;

                _remoteDesktop_UseCurrentViewSize = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_UseFixedScreenSize;
        public bool RemoteDesktop_UseFixedScreenSize
        {
            get => _remoteDesktop_UseFixedScreenSize;
            set
            {
                if (value == _remoteDesktop_UseFixedScreenSize)
                    return;

                _remoteDesktop_UseFixedScreenSize = value;
                OnPropertyChanged();
            }
        }

        public List<string> RemoteDesktop_ScreenResolutions => RemoteDesktop.ScreenResolutions;

        public int RemoteDesktop_ScreenWidth;
        public int RemoteDesktop_ScreenHeight;

        private string _remoteDesktop_SelectedScreenResolution;
        public string RemoteDesktop_SelectedScreenResolution
        {
            get => _remoteDesktop_SelectedScreenResolution;
            set
            {
                if (value == _remoteDesktop_SelectedScreenResolution)
                    return;

                var resolution = value.Split('x');

                RemoteDesktop_ScreenWidth = int.Parse(resolution[0]);
                RemoteDesktop_ScreenHeight = int.Parse(resolution[1]);

                _remoteDesktop_SelectedScreenResolution = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_UseCustomScreenSize;
        public bool RemoteDesktop_UseCustomScreenSize
        {
            get => _remoteDesktop_UseCustomScreenSize;
            set
            {
                if (value == _remoteDesktop_UseCustomScreenSize)
                    return;

                _remoteDesktop_UseCustomScreenSize = value;
                OnPropertyChanged();
            }
        }

        private string _remoteDesktop_CustomScreenWidth;
        public string RemoteDesktop_CustomScreenWidth
        {
            get => _remoteDesktop_CustomScreenWidth;
            set
            {
                if (value == _remoteDesktop_CustomScreenWidth)
                    return;

                _remoteDesktop_CustomScreenWidth = value;
                OnPropertyChanged();
            }
        }

        private string _remoteDesktop_CustomScreenHeight;
        public string RemoteDesktop_CustomScreenHeight
        {
            get => _remoteDesktop_CustomScreenHeight;
            set
            {
                if (value == _remoteDesktop_CustomScreenHeight)
                    return;

                _remoteDesktop_CustomScreenHeight = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideColorDepth;
        public bool RemoteDesktop_OverrideColorDepth
        {
            get => _remoteDesktop_OverrideColorDepth;
            set
            {
                if (value == _remoteDesktop_OverrideColorDepth)
                    return;

                _remoteDesktop_OverrideColorDepth = value;
                OnPropertyChanged();
            }
        }

        public List<int> RemoteDesktop_ColorDepths => RemoteDesktop.ColorDepths;

        private int _remoteDesktop_SelectedColorDepth;
        public int RemoteDesktop_SelectedColorDepth
        {
            get => _remoteDesktop_SelectedColorDepth;
            set
            {
                if (value == _remoteDesktop_SelectedColorDepth)
                    return;

                _remoteDesktop_SelectedColorDepth = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverridePort;
        public bool RemoteDesktop_OverridePort
        {
            get => _remoteDesktop_OverridePort;
            set
            {
                if (value == _remoteDesktop_OverridePort)
                    return;

                _remoteDesktop_OverridePort = value;
                OnPropertyChanged();
            }
        }

        private int _remoteDesktop_Port;
        public int RemoteDesktop_Port
        {
            get => _remoteDesktop_Port;
            set
            {
                if (value == _remoteDesktop_Port)
                    return;

                _remoteDesktop_Port = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideCredSspSupport;
        public bool RemoteDesktop_OverrideCredSspSupport
        {
            get => _remoteDesktop_OverrideCredSspSupport;
            set
            {
                if (value == _remoteDesktop_OverrideCredSspSupport)
                    return;

                _remoteDesktop_OverrideCredSspSupport = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_EnableCredSspSupport;
        public bool RemoteDesktop_EnableCredSspSupport
        {
            get => _remoteDesktop_EnableCredSspSupport;
            set
            {
                if (value == _remoteDesktop_EnableCredSspSupport)
                    return;

                _remoteDesktop_EnableCredSspSupport = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideAuthenticationLevel;
        public bool RemoteDesktop_OverrideAuthenticationLevel
        {
            get => _remoteDesktop_OverrideAuthenticationLevel;
            set
            {
                if (value == _remoteDesktop_OverrideAuthenticationLevel)
                    return;

                _remoteDesktop_OverrideAuthenticationLevel = value;
                OnPropertyChanged();
            }
        }

        private uint _remoteDesktop_AuthenticationLevel;
        public uint RemoteDesktop_AuthenticationLevel
        {
            get => _remoteDesktop_AuthenticationLevel;
            set
            {
                if (value == _remoteDesktop_AuthenticationLevel)
                    return;

                _remoteDesktop_AuthenticationLevel = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideAudioRedirectionMode;
        public bool RemoteDesktop_OverrideAudioRedirectionMode
        {
            get => _remoteDesktop_OverrideAudioRedirectionMode;
            set
            {
                if (value == _remoteDesktop_OverrideAudioRedirectionMode)
                    return;

                _remoteDesktop_OverrideAudioRedirectionMode = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<RemoteDesktop.AudioRedirectionMode> RemoteDesktop_AudioRedirectionModes => System.Enum.GetValues(typeof(RemoteDesktop.AudioRedirectionMode)).Cast<RemoteDesktop.AudioRedirectionMode>();

        private RemoteDesktop.AudioRedirectionMode _remoteDesktop_AudioRedirectionMode;
        public RemoteDesktop.AudioRedirectionMode RemoteDesktop_AudioRedirectionMode
        {
            get => _remoteDesktop_AudioRedirectionMode;
            set
            {
                if (Equals(value, _remoteDesktop_AudioRedirectionMode))
                    return;

                _remoteDesktop_AudioRedirectionMode = value;
                OnPropertyChanged();
            }
        }


        private bool _remoteDesktop_OverrideAudioCaptureRedirectionMode;
        public bool RemoteDesktop_OverrideAudioCaptureRedirectionMode
        {
            get => _remoteDesktop_OverrideAudioCaptureRedirectionMode;
            set
            {
                if (value == _remoteDesktop_OverrideAudioCaptureRedirectionMode)
                    return;

                _remoteDesktop_OverrideAudioCaptureRedirectionMode = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<RemoteDesktop.AudioCaptureRedirectionMode> RemoteDesktop_AudioCaptureRedirectionModes => System.Enum.GetValues(typeof(RemoteDesktop.AudioCaptureRedirectionMode)).Cast<RemoteDesktop.AudioCaptureRedirectionMode>();

        private RemoteDesktop.AudioCaptureRedirectionMode _remoteDesktop_AudioCaptureRedirectionMode;
        public RemoteDesktop.AudioCaptureRedirectionMode RemoteDesktop_AudioCaptureRedirectionMode
        {
            get => _remoteDesktop_AudioCaptureRedirectionMode;
            set
            {
                if (Equals(value, _remoteDesktop_AudioCaptureRedirectionMode))
                    return;

                _remoteDesktop_AudioCaptureRedirectionMode = value;
                OnPropertyChanged();
            }
        }


        private bool _remoteDesktop_OverrideApplyWindowsKeyCombinations;
        public bool RemoteDesktop_OverrideApplyWindowsKeyCombinations
        {
            get => _remoteDesktop_OverrideApplyWindowsKeyCombinations;
            set
            {
                if (value == _remoteDesktop_OverrideApplyWindowsKeyCombinations)
                    return;

                _remoteDesktop_OverrideApplyWindowsKeyCombinations = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<RemoteDesktop.KeyboardHookMode> RemoteDesktop_KeyboardHookModes => System.Enum.GetValues(typeof(RemoteDesktop.KeyboardHookMode)).Cast<RemoteDesktop.KeyboardHookMode>();

        private RemoteDesktop.KeyboardHookMode _remoteDesktop_KeyboardHookMode;
        public RemoteDesktop.KeyboardHookMode RemoteDesktop_KeyboardHookMode
        {
            get => _remoteDesktop_KeyboardHookMode;
            set
            {
                if (Equals(value, _remoteDesktop_KeyboardHookMode))
                    return;

                _remoteDesktop_KeyboardHookMode = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideRedirectClipboard;
        public bool RemoteDesktop_OverrideRedirectClipboard
        {
            get => _remoteDesktop_OverrideRedirectClipboard;
            set
            {
                if (value == _remoteDesktop_OverrideRedirectClipboard)
                    return;

                _remoteDesktop_OverrideRedirectClipboard = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_RedirectClipboard;
        public bool RemoteDesktop_RedirectClipboard
        {
            get => _remoteDesktop_RedirectClipboard;
            set
            {
                if (value == _remoteDesktop_RedirectClipboard)
                    return;

                _remoteDesktop_RedirectClipboard = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideRedirectDevices;
        public bool RemoteDesktop_OverrideRedirectDevices
        {
            get => _remoteDesktop_OverrideRedirectDevices;
            set
            {
                if (value == _remoteDesktop_OverrideRedirectDevices)
                    return;

                _remoteDesktop_OverrideRedirectDevices = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_RedirectDevices;
        public bool RemoteDesktop_RedirectDevices
        {
            get => _remoteDesktop_RedirectDevices;
            set
            {
                if (value == _remoteDesktop_RedirectDevices)
                    return;

                _remoteDesktop_RedirectDevices = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideRedirectDrives;
        public bool RemoteDesktop_OverrideRedirectDrives
        {
            get => _remoteDesktop_OverrideRedirectDrives;
            set
            {
                if (value == _remoteDesktop_OverrideRedirectDrives)
                    return;

                _remoteDesktop_OverrideRedirectDrives = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_RedirectDrives;
        public bool RemoteDesktop_RedirectDrives
        {
            get => _remoteDesktop_RedirectDrives;
            set
            {
                if (value == _remoteDesktop_RedirectDrives)
                    return;

                _remoteDesktop_RedirectDrives = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideRedirectPorts;
        public bool RemoteDesktop_OverrideRedirectPorts
        {
            get => _remoteDesktop_OverrideRedirectPorts;
            set
            {
                if (value == _remoteDesktop_OverrideRedirectPorts)
                    return;

                _remoteDesktop_OverrideRedirectPorts = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_RedirectPorts;
        public bool RemoteDesktop_RedirectPorts
        {
            get => _remoteDesktop_RedirectPorts;
            set
            {
                if (value == _remoteDesktop_RedirectPorts)
                    return;

                _remoteDesktop_RedirectPorts = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideRedirectSmartcards;
        public bool RemoteDesktop_OverrideRedirectSmartcards
        {
            get => _remoteDesktop_OverrideRedirectSmartcards;
            set
            {
                if (value == _remoteDesktop_OverrideRedirectSmartcards)
                    return;

                _remoteDesktop_OverrideRedirectSmartcards = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_RedirectSmartCards;
        public bool RemoteDesktop_RedirectSmartCards
        {
            get => _remoteDesktop_RedirectSmartCards;
            set
            {
                if (value == _remoteDesktop_RedirectSmartCards)
                    return;

                _remoteDesktop_RedirectSmartCards = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideRedirectPrinters;
        public bool RemoteDesktop_OverrideRedirectPrinters
        {
            get => _remoteDesktop_OverrideRedirectPrinters;
            set
            {
                if (value == _remoteDesktop_OverrideRedirectPrinters)
                    return;

                _remoteDesktop_OverrideRedirectPrinters = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_RedirectPrinters;
        public bool RemoteDesktop_RedirectPrinters
        {
            get => _remoteDesktop_RedirectPrinters;
            set
            {
                if (value == _remoteDesktop_RedirectPrinters)
                    return;

                _remoteDesktop_RedirectPrinters = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverridePersistentBitmapCaching;
        public bool RemoteDesktop_OverridePersistentBitmapCaching
        {
            get => _remoteDesktop_OverridePersistentBitmapCaching;
            set
            {
                if (value == _remoteDesktop_OverridePersistentBitmapCaching)
                    return;

                _remoteDesktop_OverridePersistentBitmapCaching = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_PersistentBitmapCaching;
        public bool RemoteDesktop_PersistentBitmapCaching
        {
            get => _remoteDesktop_PersistentBitmapCaching;
            set
            {
                if (value == _remoteDesktop_PersistentBitmapCaching)
                    return;

                _remoteDesktop_PersistentBitmapCaching = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
        public bool RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped
        {
            get => _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
            set
            {
                if (value == _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped)
                    return;

                _remoteDesktop_OverrideReconnectIfTheConnectionIsDropped = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_ReconnectIfTheConnectionIsDropped;
        public bool RemoteDesktop_ReconnectIfTheConnectionIsDropped
        {
            get => _remoteDesktop_ReconnectIfTheConnectionIsDropped;
            set
            {
                if (value == _remoteDesktop_ReconnectIfTheConnectionIsDropped)
                    return;

                _remoteDesktop_ReconnectIfTheConnectionIsDropped = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_OverrideNetworkConnectionType;
        public bool RemoteDesktop_OverrideNetworkConnectionType
        {
            get => _remoteDesktop_OverrideNetworkConnectionType;
            set
            {
                if (value == _remoteDesktop_OverrideNetworkConnectionType)
                    return;

                _remoteDesktop_OverrideNetworkConnectionType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<RemoteDesktop.NetworkConnectionType> RemoteDesktop_NetworkConnectionTypes => System.Enum.GetValues(typeof(RemoteDesktop.NetworkConnectionType)).Cast<RemoteDesktop.NetworkConnectionType>();

        private RemoteDesktop.NetworkConnectionType _remoteDesktop_NetworkConnectionType;
        public RemoteDesktop.NetworkConnectionType RemoteDesktop_NetworkConnectionType
        {
            get => _remoteDesktop_NetworkConnectionType;
            set
            {
                if (Equals(value, _remoteDesktop_NetworkConnectionType))
                    return;

                if (!_isLoading)
                    ChangeNetworkConnectionTypeSettings(value);

                _remoteDesktop_NetworkConnectionType = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_DesktopBackground;
        public bool RemoteDesktop_DesktopBackground
        {
            get => _remoteDesktop_DesktopBackground;
            set
            {
                if (value == _remoteDesktop_DesktopBackground)
                    return;

                _remoteDesktop_DesktopBackground = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_FontSmoothing;
        public bool RemoteDesktop_FontSmoothing
        {
            get => _remoteDesktop_FontSmoothing;
            set
            {
                if (value == _remoteDesktop_FontSmoothing)
                    return;

                _remoteDesktop_FontSmoothing = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_DesktopComposition;
        public bool RemoteDesktop_DesktopComposition
        {
            get => _remoteDesktop_DesktopComposition;
            set
            {
                if (value == _remoteDesktop_DesktopComposition)
                    return;

                _remoteDesktop_DesktopComposition = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_ShowWindowContentsWhileDragging;
        public bool RemoteDesktop_ShowWindowContentsWhileDragging
        {
            get => _remoteDesktop_ShowWindowContentsWhileDragging;
            set
            {
                if (value == _remoteDesktop_ShowWindowContentsWhileDragging)
                    return;

                _remoteDesktop_ShowWindowContentsWhileDragging = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_MenuAndWindowAnimation;
        public bool RemoteDesktop_MenuAndWindowAnimation
        {
            get => _remoteDesktop_MenuAndWindowAnimation;
            set
            {
                if (value == _remoteDesktop_MenuAndWindowAnimation)
                    return;

                _remoteDesktop_MenuAndWindowAnimation = value;
                OnPropertyChanged();
            }
        }

        private bool _remoteDesktop_VisualStyles;
        public bool RemoteDesktop_VisualStyles
        {
            get => _remoteDesktop_VisualStyles;
            set
            {
                if (value == _remoteDesktop_VisualStyles)
                    return;

                _remoteDesktop_VisualStyles = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region PowerShell
        private bool _powerShell_Enabled;
        public bool PowerShell_Enabled
        {
            get => _powerShell_Enabled;
            set
            {
                if (value == _powerShell_Enabled)
                    return;

                _powerShell_Enabled = value;

                OnPropertyChanged();
            }
        }

        private bool _powerShell_EnableRemoteConsole;
        public bool PowerShell_EnableRemoteConsole
        {
            get => _powerShell_EnableRemoteConsole;
            set
            {
                if (value == _powerShell_EnableRemoteConsole)
                    return;

                _powerShell_EnableRemoteConsole = value;
                OnPropertyChanged();
            }
        }

        private bool _powerShell_InheritHost;
        public bool PowerShell_InheritHost
        {
            get => _powerShell_InheritHost;
            set
            {
                if (value == _powerShell_InheritHost)
                    return;

                _powerShell_InheritHost = value;
                OnPropertyChanged();
            }
        }

        private string _powerShell_Host;
        public string PowerShell_Host
        {
            get => _powerShell_Host;
            set
            {
                if (value == _powerShell_Host)
                    return;

                _powerShell_Host = value;
                OnPropertyChanged();
            }
        }

        private bool _powerShell_OverrideAdditionalCommandLine;
        public bool PowerShell_OverrideAdditionalCommandLine
        {
            get => _powerShell_OverrideAdditionalCommandLine;
            set
            {
                if (value == _powerShell_OverrideAdditionalCommandLine)
                    return;

                _powerShell_OverrideAdditionalCommandLine = value;
                OnPropertyChanged();
            }
        }

        private string _powerShell_AdditionalCommandLine;
        public string PowerShell_AdditionalCommandLine
        {
            get => _powerShell_AdditionalCommandLine;
            set
            {
                if (value == _powerShell_AdditionalCommandLine)
                    return;

                _powerShell_AdditionalCommandLine = value;
                OnPropertyChanged();
            }
        }

        private List<PowerShell.ExecutionPolicy> _powerShell_ExecutionPolicies = new List<PowerShell.ExecutionPolicy>();
        public List<PowerShell.ExecutionPolicy> PowerShell_ExecutionPolicies
        {
            get => _powerShell_ExecutionPolicies;
            set
            {
                if (value == _powerShell_ExecutionPolicies)
                    return;

                _powerShell_ExecutionPolicies = value;
                OnPropertyChanged();
            }
        }

        private bool _powerShell_OverrideExecutionPolicy;
        public bool PowerShell_OverrideExecutionPolicy
        {
            get => _powerShell_OverrideExecutionPolicy;
            set
            {
                if (value == _powerShell_OverrideExecutionPolicy)
                    return;

                _powerShell_OverrideExecutionPolicy = value;
                OnPropertyChanged();
            }
        }

        private PowerShell.ExecutionPolicy _powerShell_ExecutionPolicy;
        public PowerShell.ExecutionPolicy PowerShell_ExecutionPolicy
        {
            get => _powerShell_ExecutionPolicy;
            set
            {
                if (value == _powerShell_ExecutionPolicy)
                    return;

                _powerShell_ExecutionPolicy = value;
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
                    if (PuTTY_ConnectionMode == ConnectionMode.Serial)
                        PuTTY_HostOrSerialLine = Host;

                    PuTTY_PortOrBaud = SettingsManager.Current.PuTTY_SSHPort;
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
                    if (PuTTY_ConnectionMode == ConnectionMode.Serial)
                        PuTTY_HostOrSerialLine = Host;

                    PuTTY_PortOrBaud = SettingsManager.Current.PuTTY_TelnetPort;
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
                    if (PuTTY_ConnectionMode != ConnectionMode.Serial)
                        PuTTY_HostOrSerialLine = SettingsManager.Current.PuTTY_SerialLine;

                    PuTTY_PortOrBaud = SettingsManager.Current.PuTTY_BaudRate;
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
                    if (PuTTY_ConnectionMode == ConnectionMode.Serial)
                        PuTTY_HostOrSerialLine = Host;

                    PuTTY_PortOrBaud = SettingsManager.Current.PuTTY_RloginPort;
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
                    if (PuTTY_ConnectionMode == ConnectionMode.Serial)
                        PuTTY_HostOrSerialLine = Host;

                    PuTTY_PortOrBaud = 0;
                    PuTTY_ConnectionMode = ConnectionMode.RAW;
                }

                _puTTY_UseRAW = value;
                OnPropertyChanged();
            }
        }

        private string _puTTY_HostOrSerialLine;
        public string PuTTY_HostOrSerialLine
        {
            get => _puTTY_HostOrSerialLine;
            set
            {
                if (value == _puTTY_HostOrSerialLine)
                    return;

                _puTTY_HostOrSerialLine = value;
                OnPropertyChanged();
            }
        }

        private bool _puTTY_OverridePortOrBaud;
        public bool PuTTY_OverridePortOrBaud
        {
            get => _puTTY_OverridePortOrBaud;
            set
            {
                if (value == _puTTY_OverridePortOrBaud)
                    return;

                _puTTY_OverridePortOrBaud = value;
                OnPropertyChanged();
            }
        }

        private int _puTTY_PortOrBaud;
        public int PuTTY_PortOrBaud
        {
            get => _puTTY_PortOrBaud;
            set
            {
                if (value == _puTTY_PortOrBaud)
                    return;

                _puTTY_PortOrBaud = value;
                OnPropertyChanged();
            }
        }

        private bool _puTTY_OverrideUsername;
        public bool PuTTY_OverrideUsername
        {
            get => _puTTY_OverrideUsername;
            set
            {
                if (value == _puTTY_OverrideUsername)
                    return;

                _puTTY_OverrideUsername = value;
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
                OnPropertyChanged();
            }
        }

        private bool _puTTY_OverrideProfile;
        public bool PuTTY_OverrideProfile
        {
            get => _puTTY_OverrideProfile;
            set
            {
                if (value == _puTTY_OverrideProfile)
                    return;

                _puTTY_OverrideProfile = value;
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
                OnPropertyChanged();
            }
        }

        private bool _puTTY_OverrideAdditionalCommandLine;
        public bool PuTTY_OverrideAdditionalCommandLine
        {
            get => _puTTY_OverrideAdditionalCommandLine;
            set
            {
                if (value == _puTTY_OverrideAdditionalCommandLine)
                    return;

                _puTTY_OverrideAdditionalCommandLine = value;
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
            }
        }
        #endregion

        #region TigerVNC
        private bool _tigerVNC_Enabled;
        public bool TigerVNC_Enabled
        {
            get => _tigerVNC_Enabled;
            set
            {
                if (value == _tigerVNC_Enabled)
                    return;

                _tigerVNC_Enabled = value;

                OnPropertyChanged();
            }
        }

        private bool _tigerVNC_InheritHost;
        public bool TigerVNC_InheritHost
        {
            get => _tigerVNC_InheritHost;
            set
            {
                if (value == _tigerVNC_InheritHost)
                    return;

                _tigerVNC_InheritHost = value;
                OnPropertyChanged();
            }
        }

        private string _tigerVNC_Host;
        public string TigerVNC_Host
        {
            get => _tigerVNC_Host;
            set
            {
                if (value == _tigerVNC_Host)
                    return;

                _tigerVNC_Host = value;
                OnPropertyChanged();
            }
        }


        private bool _tigerVNC_OverridePort;
        public bool TigerVNC_OverridePort
        {
            get => _tigerVNC_OverridePort;
            set
            {
                if (value == _tigerVNC_OverridePort)
                    return;

                _tigerVNC_OverridePort = value;
                OnPropertyChanged();
            }
        }

        private int _tigerVNC_Port;
        public int TigerVNC_Port
        {
            get => _tigerVNC_Port;
            set
            {
                if (value == _tigerVNC_Port)
                    return;

                _tigerVNC_Port = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private bool _wakeOnLAN_OverridePort;
        public bool WakeOnLAN_OverridePort
        {
            get => _wakeOnLAN_OverridePort;
            set
            {
                if (value == _wakeOnLAN_OverridePort)
                    return;

                _wakeOnLAN_OverridePort = value;
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
                OnPropertyChanged();
            }
        }

        private string _whois_Domain;
        public string Whois_Domain
        {
            get => _whois_Domain;
            set
            {
                if (value == _whois_Domain)
                    return;

                _whois_Domain = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        public ProfileViewModel(Action<ProfileViewModel> saveCommand, Action<ProfileViewModel> cancelHandler, IReadOnlyCollection<string> groups, ProfileEditMode editMode = ProfileEditMode.Add, ProfileInfo profile = null)
        {
            _isLoading = true;

            // Load the view
            ProfileViews = new CollectionViewSource { Source = ProfileViewManager.List }.View;
            ProfileViews.SortDescriptions.Add(new SortDescription(nameof(ProfileViewInfo.Name), ListSortDirection.Ascending));

            SaveCommand = new RelayCommand(p => saveCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            var profileInfo = profile ?? new ProfileInfo();

            Name = profileInfo.Name;

            if (editMode == ProfileEditMode.Copy)
                Name += " - " + Resources.Localization.Strings.CopyNoun;

            Host = profileInfo.Host;
            Group = string.IsNullOrEmpty(profileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : Resources.Localization.Strings.Default) : profileInfo.Group;
            Tags = profileInfo.Tags;

            Groups = CollectionViewSource.GetDefaultView(groups);
            Groups.SortDescriptions.Add(new SortDescription());

            // Network Interface
            NetworkInterface_Enabled = profileInfo.NetworkInterface_Enabled;
            NetworkInterface_EnableDynamicIPAddress = !profileInfo.NetworkInterface_EnableStaticIPAddress;
            NetworkInterface_EnableStaticIPAddress = profileInfo.NetworkInterface_EnableStaticIPAddress;
            NetworkInterface_IPAddress = profileInfo.NetworkInterface_IPAddress;
            NetworkInterface_Gateway = profileInfo.NetworkInterface_Gateway;
            NetworkInterface_SubnetmaskOrCidr = profileInfo.NetworkInterface_SubnetmaskOrCidr;
            NetworkInterface_EnableDynamicDNS = !profileInfo.NetworkInterface_EnableStaticDNS;
            NetworkInterface_EnableStaticDNS = profileInfo.NetworkInterface_EnableStaticDNS;
            NetworkInterface_PrimaryDNSServer = profileInfo.NetworkInterface_PrimaryDNSServer;
            NetworkInterface_SecondaryDNSServer = profileInfo.NetworkInterface_SecondaryDNSServer;

            // IP Scanner
            IPScanner_Enabled = profileInfo.IPScanner_Enabled;
            IPScanner_InheritHost = profileInfo.IPScanner_InheritHost;
            IPScanner_HostOrIPRange = profileInfo.IPScanner_HostOrIPRange;

            // Port Scanner
            PortScanner_Enabled = profileInfo.PortScanner_Enabled;
            PortScanner_InheritHost = profileInfo.PortScanner_InheritHost;
            PortScanner_Host = profileInfo.PortScanner_Host;
            PortScanner_Ports = profileInfo.PortScanner_Ports;

            // Ping
            Ping_Enabled = profileInfo.Ping_Enabled;
            Ping_InheritHost = profileInfo.Ping_InheritHost;
            Ping_Host = profileInfo.Ping_Host;

            // Ping Monitor
            PingMonitor_Enabled = profileInfo.PingMonitor_Enabled;
            PingMonitor_InheritHost = profileInfo.PingMonitor_InheritHost;
            PingMonitor_Host = profileInfo.PingMonitor_Host;

            // Traceroute
            Traceroute_Enabled = profileInfo.Traceroute_Enabled;
            Traceroute_InheritHost = profileInfo.Traceroute_InheritHost;
            Traceroute_Host = profileInfo.Traceroute_Host;

            // DNS Lookup
            DNSLookup_Enabled = profileInfo.DNSLookup_Enabled;
            DNSLookup_InheritHost = profileInfo.DNSLookup_InheritHost;
            DNSLookup_Host = profileInfo.DNSLookup_Host;

            // Remote Desktop
            RemoteDesktop_Enabled = profileInfo.RemoteDesktop_Enabled;
            RemoteDesktop_InheritHost = profileInfo.RemoteDesktop_InheritHost;
            RemoteDesktop_Host = profileInfo.RemoteDesktop_Host;
            RemoteDesktop_OverrideDisplay = profileInfo.RemoteDesktop_OverrideDisplay;
            RemoteDesktop_AdjustScreenAutomatically = profileInfo.RemoteDesktop_AdjustScreenAutomatically;
            RemoteDesktop_UseCurrentViewSize = profileInfo.RemoteDesktop_UseCurrentViewSize;
            RemoteDesktop_UseFixedScreenSize = profileInfo.RemoteDesktop_UseFixedScreenSize;
            RemoteDesktop_SelectedScreenResolution = RemoteDesktop_ScreenResolutions.FirstOrDefault(x => x == $"{profileInfo.RemoteDesktop_ScreenWidth}x{profileInfo.RemoteDesktop_ScreenHeight}");
            RemoteDesktop_UseCustomScreenSize = profileInfo.RemoteDesktop_UseCustomScreenSize;
            RemoteDesktop_CustomScreenWidth = profileInfo.RemoteDesktop_CustomScreenWidth.ToString();
            RemoteDesktop_CustomScreenHeight = profileInfo.RemoteDesktop_CustomScreenHeight.ToString();
            RemoteDesktop_OverrideColorDepth = profileInfo.RemoteDesktop_OverrideColorDepth;
            RemoteDesktop_SelectedColorDepth = RemoteDesktop_ColorDepths.FirstOrDefault(x => x == profileInfo.RemoteDesktop_ColorDepth);
            RemoteDesktop_OverridePort = profileInfo.RemoteDesktop_OverridePort;
            RemoteDesktop_Port = profileInfo.RemoteDesktop_Port;
            RemoteDesktop_OverrideCredSspSupport = profileInfo.RemoteDesktop_OverrideCredSspSupport;
            RemoteDesktop_EnableCredSspSupport = profileInfo.RemoteDesktop_EnableCredSspSupport;
            RemoteDesktop_OverrideAuthenticationLevel = profileInfo.RemoteDesktop_OverrideAuthenticationLevel;
            RemoteDesktop_AuthenticationLevel = profileInfo.RemoteDesktop_AuthenticationLevel;
            RemoteDesktop_OverrideAudioRedirectionMode = profileInfo.RemoteDesktop_OverrideAudioRedirectionMode;
            RemoteDesktop_AudioRedirectionMode = RemoteDesktop_AudioRedirectionModes.FirstOrDefault(x => x == profileInfo.RemoteDesktop_AudioRedirectionMode);
            RemoteDesktop_OverrideAudioCaptureRedirectionMode = profileInfo.RemoteDesktop_OverrideAudioCaptureRedirectionMode;
            RemoteDesktop_AudioCaptureRedirectionMode = RemoteDesktop_AudioCaptureRedirectionModes.FirstOrDefault(x => x == profileInfo.RemoteDesktop_AudioCaptureRedirectionMode);
            RemoteDesktop_OverrideApplyWindowsKeyCombinations = profileInfo.RemoteDesktop_OverrideApplyWindowsKeyCombinations;
            RemoteDesktop_KeyboardHookMode = RemoteDesktop_KeyboardHookModes.FirstOrDefault(x => x == profileInfo.RemoteDesktop_KeyboardHookMode);
            RemoteDesktop_OverrideRedirectClipboard = profileInfo.RemoteDesktop_OverrideRedirectClipboard;
            RemoteDesktop_RedirectClipboard = profileInfo.RemoteDesktop_RedirectClipboard;
            RemoteDesktop_OverrideRedirectDevices = profileInfo.RemoteDesktop_OverrideRedirectDevices;
            RemoteDesktop_RedirectDevices = profileInfo.RemoteDesktop_RedirectDevices;
            RemoteDesktop_OverrideRedirectDrives = profileInfo.RemoteDesktop_OverrideRedirectDrives;
            RemoteDesktop_RedirectDrives = profileInfo.RemoteDesktop_RedirectDrives;
            RemoteDesktop_OverrideRedirectPorts = profileInfo.RemoteDesktop_OverrideRedirectPorts;
            RemoteDesktop_RedirectPorts = profileInfo.RemoteDesktop_RedirectPorts;
            RemoteDesktop_OverrideRedirectSmartcards = profileInfo.RemoteDesktop_OverrideRedirectSmartcards;
            RemoteDesktop_RedirectSmartCards = profileInfo.RemoteDesktop_RedirectSmartCards;
            RemoteDesktop_OverrideRedirectPrinters = profileInfo.RemoteDesktop_OverrideRedirectPrinters;
            RemoteDesktop_RedirectPrinters = profileInfo.RemoteDesktop_RedirectPrinters;
            RemoteDesktop_OverridePersistentBitmapCaching = profileInfo.RemoteDesktop_OverridePersistentBitmapCaching;
            RemoteDesktop_PersistentBitmapCaching = profileInfo.RemoteDesktop_PersistentBitmapCaching;
            RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped = profileInfo.RemoteDesktop_OverrideReconnectIfTheConnectionIsDropped;
            RemoteDesktop_ReconnectIfTheConnectionIsDropped = profileInfo.RemoteDesktop_ReconnectIfTheConnectionIsDropped;
            RemoteDesktop_NetworkConnectionType = RemoteDesktop_NetworkConnectionTypes.FirstOrDefault(x => x == profileInfo.RemoteDesktop_NetworkConnectionType);
            RemoteDesktop_DesktopBackground = profileInfo.RemoteDesktop_DesktopBackground;
            RemoteDesktop_FontSmoothing = profileInfo.RemoteDesktop_FontSmoothing;
            RemoteDesktop_DesktopComposition = profileInfo.RemoteDesktop_DesktopComposition;
            RemoteDesktop_ShowWindowContentsWhileDragging = profileInfo.RemoteDesktop_ShowWindowContentsWhileDragging;
            RemoteDesktop_MenuAndWindowAnimation = profileInfo.RemoteDesktop_MenuAndWindowAnimation;
            RemoteDesktop_VisualStyles = profileInfo.RemoteDesktop_VisualStyles;

            // PowerShell
            PowerShell_Enabled = profileInfo.PowerShell_Enabled;
            PowerShell_EnableRemoteConsole = profileInfo.PowerShell_EnableRemoteConsole;
            PowerShell_InheritHost = profileInfo.PowerShell_InheritHost;
            PowerShell_Host = profileInfo.PowerShell_Host;
            PowerShell_OverrideAdditionalCommandLine = profileInfo.PowerShell_OverrideAdditionalCommandLine;
            PowerShell_AdditionalCommandLine = profileInfo.PowerShell_AdditionalCommandLine;
            PowerShell_ExecutionPolicies = System.Enum.GetValues(typeof(PowerShell.ExecutionPolicy)).Cast<PowerShell.ExecutionPolicy>().ToList();
            PowerShell_OverrideExecutionPolicy = profileInfo.PowerShell_OverrideExecutionPolicy;
            PowerShell_ExecutionPolicy = editMode != ProfileEditMode.Add ? profileInfo.PowerShell_ExecutionPolicy : PowerShell_ExecutionPolicies.FirstOrDefault(x => x == SettingsManager.Current.PowerShell_ExecutionPolicy); ;

            // PuTTY
            PuTTY_Enabled = profileInfo.PuTTY_Enabled;

            switch (profileInfo.PuTTY_ConnectionMode)
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

            PuTTY_InheritHost = profileInfo.PuTTY_InheritHost;
            PuTTY_HostOrSerialLine = profileInfo.PuTTY_HostOrSerialLine;
            PuTTY_OverridePortOrBaud = profileInfo.PuTTY_OverridePortOrBaud;
            PuTTY_PortOrBaud = profileInfo.PuTTY_OverridePortOrBaud ? profileInfo.PuTTY_PortOrBaud : GetPortOrBaudByConnectionMode(PuTTY_ConnectionMode);
            PuTTY_OverrideUsername = profileInfo.PuTTY_OverrideUsername;
            PuTTY_Username = profileInfo.PuTTY_Username;
            PuTTY_OverrideProfile = profileInfo.PuTTY_OverrideProfile;
            PuTTY_Profile = profileInfo.PuTTY_Profile;
            PuTTY_OverrideAdditionalCommandLine = profileInfo.PuTTY_OverrideAdditionalCommandLine;
            PuTTY_AdditionalCommandLine = profileInfo.PuTTY_AdditionalCommandLine;

            // TigerVNC
            TigerVNC_Enabled = profileInfo.TigerVNC_Enabled;
            TigerVNC_InheritHost = profileInfo.TigerVNC_InheritHost;
            TigerVNC_Host = profileInfo.TigerVNC_Host;
            TigerVNC_OverridePort = profileInfo.TigerVNC_OverridePort;
            TigerVNC_Port = profileInfo.TigerVNC_OverridePort ? profileInfo.TigerVNC_Port : SettingsManager.Current.TigerVNC_Port;

            // Wake on LAN
            WakeOnLAN_Enabled = profileInfo.WakeOnLAN_Enabled;
            WakeOnLAN_MACAddress = profileInfo.WakeOnLAN_MACAddress;
            WakeOnLAN_Broadcast = profileInfo.WakeOnLAN_Broadcast;
            WakeOnLAN_OverridePort = profileInfo.WakeOnLAN_OverridePort;
            WakeOnLAN_Port = profileInfo.WakeOnLAN_OverridePort ? profileInfo.WakeOnLAN_Port : SettingsManager.Current.WakeOnLAN_Port;

            // HTTP Headers
            HTTPHeaders_Enabled = profileInfo.HTTPHeaders_Enabled;
            HTTPHeaders_Website = profileInfo.HTTPHeaders_Website;

            // Whois
            Whois_Enabled = profileInfo.Whois_Enabled;
            Whois_InheritHost = profileInfo.Whois_InheritHost;
            Whois_Domain = profileInfo.Whois_Domain;

            _isLoading = false;
        }

        #region ICommands & Actions
        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ResolveHostCommand => new RelayCommand(async p => await ResolveHostActionAsync());

        private async System.Threading.Tasks.Task ResolveHostActionAsync()
        {
            IsResolveHostnameRunning = true;

            try
            {
                foreach (var ipAddr in (await Dns.GetHostEntryAsync(Host)).AddressList)
                {
                    if (ipAddr.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    Host = ipAddr.ToString();
                    break;
                }

            }
            catch (SocketException) // DNS Error
            {
                ShowCouldNotResolveHostnameWarning = true;
            }

            IsResolveHostnameRunning = false;
        }
        #endregion

        #region Methods
        private void ChangeNetworkConnectionTypeSettings(RemoteDesktop.NetworkConnectionType connectionSpeed)
        {
            switch (connectionSpeed)
            {
                case RemoteDesktop.NetworkConnectionType.Modem:
                    RemoteDesktop_DesktopBackground = false;
                    RemoteDesktop_FontSmoothing = false;
                    RemoteDesktop_DesktopComposition = false;
                    RemoteDesktop_ShowWindowContentsWhileDragging = false;
                    RemoteDesktop_MenuAndWindowAnimation = false;
                    RemoteDesktop_VisualStyles = false;
                    break;
                case RemoteDesktop.NetworkConnectionType.BroadbandLow:
                    RemoteDesktop_DesktopBackground = false;
                    RemoteDesktop_FontSmoothing = false;
                    RemoteDesktop_DesktopComposition = false;
                    RemoteDesktop_ShowWindowContentsWhileDragging = false;
                    RemoteDesktop_MenuAndWindowAnimation = false;
                    RemoteDesktop_VisualStyles = true;
                    break;
                case RemoteDesktop.NetworkConnectionType.Satellite:
                case RemoteDesktop.NetworkConnectionType.BroadbandHigh:
                    RemoteDesktop_DesktopBackground = false;
                    RemoteDesktop_FontSmoothing = false;
                    RemoteDesktop_DesktopComposition = true;
                    RemoteDesktop_ShowWindowContentsWhileDragging = false;
                    RemoteDesktop_MenuAndWindowAnimation = false;
                    RemoteDesktop_VisualStyles = true;
                    break;
                case RemoteDesktop.NetworkConnectionType.WAN:
                case RemoteDesktop.NetworkConnectionType.LAN:
                    RemoteDesktop_DesktopBackground = true;
                    RemoteDesktop_FontSmoothing = true;
                    RemoteDesktop_DesktopComposition = true;
                    RemoteDesktop_ShowWindowContentsWhileDragging = true;
                    RemoteDesktop_MenuAndWindowAnimation = true;
                    RemoteDesktop_VisualStyles = true;
                    break;
            }
        }
        #endregion
    }
}