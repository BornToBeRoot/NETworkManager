using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

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

        private int? _credentialID = null;
        public int? CredentialID
        {
            get { return _credentialID; }
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

        private bool _displayNote;
        public bool DisplayNote
        {
            get { return _displayNote; }
            set
            {
                if (value == _displayNote)
                    return;

                _displayNote = value;
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
            Group = string.IsNullOrEmpty(_profileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : LocalizationManager.GetStringByKey("String_Default")) : _profileInfo.Group;
            Tags = _profileInfo.Tags;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            IPScanner_Enabled = _profileInfo.IPScanner_Enabled;
            IPScanner_InheritHost = _profileInfo.IPScanner_InheritHost;
            IPScanner_IPRange = _profileInfo.IPScanner_IPRange;

            PortScanner_Enabled = _profileInfo.PortScanner_Enabled;
            PortScanner_InheritHost = _profileInfo.PortScanner_InheritHost;
            PortScanner_Host = _profileInfo.PortScanner_Host;
            PortScanner_Ports = _profileInfo.PortScanner_Ports;

            Ping_Enabled = _profileInfo.Ping_Enabled;
            Ping_InheritHost = _profileInfo.Ping_InheritHost;
            Ping_Host = _profileInfo.Ping_Host;

            Traceroute_Enabled = _profileInfo.Traceroute_Enabled;
            Traceroute_InheritHost = _profileInfo.Traceroute_InheritHost;
            Traceroute_Host = _profileInfo.Traceroute_Host;

            RemoteDesktop_Enabled = _profileInfo.RemoteDesktop_Enabled;
            RemoteDesktop_InheritHost = _profileInfo.RemoteDesktop_InheritHost;
            RemoteDesktop_Host = _profileInfo.RemoteDesktop_Host;

            Validate();

            _isLoading = false;
        }

        private void Validate()
        {
            // Note
            DisplayNote = (NetworkInterface_Enabled || IPScanner_Enabled || PortScanner_Enabled || Ping_Enabled || Traceroute_Enabled || RemoteDesktop_Enabled || WakeOnLAN_Enabled);
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
        #endregion
    }
}