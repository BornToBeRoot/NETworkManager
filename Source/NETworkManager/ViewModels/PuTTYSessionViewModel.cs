using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.ViewModels
{
    public class PuTTYProfileViewModel : ViewModelBase
    {
        private bool _isLoading = true;

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
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private bool _useSSH; // Default is SSH
        public bool UseSSH
        {
            get { return _useSSH; }
            set
            {
                if (value == _useSSH)
                    return;

                if (value)
                {
                    Port = SettingsManager.Current.PuTTY_SSHPort;
                    ConnectionMode = ConnectionMode.SSH;
                }

                _useSSH = value;
                OnPropertyChanged();
            }
        }

        private bool _useTelnet;
        public bool UseTelnet
        {
            get { return _useTelnet; }
            set
            {
                if (value == _useTelnet)
                    return;

                if (value)
                {
                    Port = SettingsManager.Current.PuTTY_TelnetPort;
                    ConnectionMode = ConnectionMode.Telnet;
                }

                _useTelnet = value;
                OnPropertyChanged();
            }
        }

        private bool _useSerial;
        public bool UseSerial
        {
            get { return _useSerial; }
            set
            {
                if (value == _useSerial)
                    return;

                if (value)
                {
                    Baud = SettingsManager.Current.PuTTY_BaudRate;
                    ConnectionMode = ConnectionMode.Serial;
                }

                _useSerial = value;
                OnPropertyChanged();
            }
        }

        private bool _useRlogin;
        public bool UseRlogin
        {
            get { return _useRlogin; }
            set
            {
                if (value == _useRlogin)
                    return;

                if (value)
                {
                    Port = SettingsManager.Current.PuTTY_RloginPort;
                    ConnectionMode = ConnectionMode.Rlogin;
                }

                _useRlogin = value;
                OnPropertyChanged();
            }
        }

        private bool _useRAW;
        public bool UseRAW
        {
            get { return _useRAW; }
            set
            {
                if (value == _useRAW)
                    return;

                if (value)
                {
                    Port = 0;
                    ConnectionMode = ConnectionMode.RAW;
                }

                _useRAW = value;
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
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _serialLine;
        public string SerialLine
        {
            get { return _serialLine; }
            set
            {
                if (value == _serialLine)
                    return;

                _serialLine = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                if (value == _port)
                    return;

                _port = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private int _baud;
        public int Baud
        {
            get { return _baud; }
            set
            {
                if (value == _baud)
                    return;

                _baud = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username)
                    return;

                _username = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _profile;
        public string Profile
        {
            get { return _profile; }
            set
            {
                if (value == _profile)
                    return;

                _profile = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private string _additionalCommandLine;
        public string AdditionalCommandLine
        {
            get { return _additionalCommandLine; }
            set
            {
                if (value == _additionalCommandLine)
                    return;

                _additionalCommandLine = value;

                if (!_isLoading)
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
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
                    HasProfileInfoChanged();

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
                    HasProfileInfoChanged();

                OnPropertyChanged();
            }
        }

        private ConnectionMode _connectionMode;
        public ConnectionMode ConnectionMode
        {
            get { return _connectionMode; }
            set
            {
                if (value == _connectionMode)
                    return;

                _connectionMode = value;

                if (!_isLoading)
                    HasProfileInfoChanged();
            }
        }

        private PuTTYProfileInfo _ProfileInfo;

        private bool _ProfileInfoChanged;
        public bool ProfileInfoChanged
        {
            get { return _ProfileInfoChanged; }
            set
            {
                if (value == _ProfileInfoChanged)
                    return;

                _ProfileInfoChanged = value;
                OnPropertyChanged();
            }
        }

        public PuTTYProfileViewModel(Action<PuTTYProfileViewModel> saveCommand, Action<PuTTYProfileViewModel> cancelHandler, List<string> groups, PuTTYProfileInfo ProfileInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _ProfileInfo = ProfileInfo ?? new PuTTYProfileInfo();

            Name = _ProfileInfo.Name;

            switch (_ProfileInfo.ConnectionMode)
            {
                // SSH is default
                case ConnectionMode.SSH:
                    UseSSH = true;
                    break;
                case ConnectionMode.Telnet:
                    UseTelnet = true;
                    break;
                case ConnectionMode.Serial:
                    UseSerial = true;
                    break;
                case ConnectionMode.Rlogin:
                    UseRlogin = true;
                    break;
                case ConnectionMode.RAW:
                    UseRAW = true;
                    break;
            }

            if (_ProfileInfo.ConnectionMode == ConnectionMode.Serial)
            {
                SerialLine = ProfileInfo.HostOrSerialLine;
                Baud = ProfileInfo.PortOrBaud;
            }
            else
            {
                Host = _ProfileInfo.HostOrSerialLine;
                Port = _ProfileInfo.PortOrBaud == 0 ? SettingsManager.Current.PuTTY_SSHPort : _ProfileInfo.PortOrBaud; // Default SSH port
            }

            Username = _ProfileInfo.Username;
            Profile = _ProfileInfo.Profile;
            AdditionalCommandLine = _ProfileInfo.AdditionalCommandLine;

            // Get the group, if not --> get the first group (ascending), fallback --> default group 
            Group = string.IsNullOrEmpty(_ProfileInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : LocalizationManager.GetStringByKey("String_Default")) : _ProfileInfo.Group;
            Tags = _ProfileInfo.Tags;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());
            
            _isLoading = false;
        }

        private void HasProfileInfoChanged()
        {
            if (ConnectionMode == ConnectionMode.Serial)
                ProfileInfoChanged = (_ProfileInfo.Name != Name) || (_ProfileInfo.ConnectionMode != ConnectionMode) || (_ProfileInfo.HostOrSerialLine != SerialLine) || (_ProfileInfo.PortOrBaud != Baud) || (_ProfileInfo.Username != Username) || (_ProfileInfo.Profile != Profile) || (_ProfileInfo.AdditionalCommandLine != AdditionalCommandLine) || (_ProfileInfo.Group != Group) || (_ProfileInfo.Tags != Tags);
            else
                ProfileInfoChanged = (_ProfileInfo.Name != Name) || (_ProfileInfo.ConnectionMode != ConnectionMode) || (_ProfileInfo.HostOrSerialLine != Host) || (_ProfileInfo.PortOrBaud != Port) || (_ProfileInfo.Username != Username) || (_ProfileInfo.Profile != Profile) || (_ProfileInfo.AdditionalCommandLine != AdditionalCommandLine) || (_ProfileInfo.Group != Group) || (_ProfileInfo.Tags != Tags);
        }
    }
}