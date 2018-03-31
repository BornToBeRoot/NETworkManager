using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.ViewModels
{
    public class PuTTYSessionViewModel : ViewModelBase
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
                    HasSessionInfoChanged();

                OnPropertyChanged();
            }
        }

        private bool _useSSH = true; // Default is SSH
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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();

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
                    HasSessionInfoChanged();
            }
        }

        private PuTTYSessionInfo _sessionInfo;

        private bool _sessionInfoChanged;
        public bool SessionInfoChanged
        {
            get { return _sessionInfoChanged; }
            set
            {
                if (value == _sessionInfoChanged)
                    return;

                _sessionInfoChanged = value;
                OnPropertyChanged();
            }
        }

        public PuTTYSessionViewModel(Action<PuTTYSessionViewModel> saveCommand, Action<PuTTYSessionViewModel> cancelHandler, List<string> groups, PuTTYSessionInfo sessionInfo = null)
        {
            _saveCommand = new RelayCommand(p => saveCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _sessionInfo = sessionInfo ?? new PuTTYSessionInfo();

            Name = _sessionInfo.Name;

            switch (_sessionInfo.ConnectionMode)
            {
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

            if (_sessionInfo.ConnectionMode == ConnectionMode.Serial)
            {
                SerialLine = sessionInfo.SerialLine;
                Baud = sessionInfo.Baud;
            }
            else
            {
                Host = _sessionInfo.Host;
                Port = _sessionInfo.Port;
            }

            Username = _sessionInfo.Username;
            Profile = _sessionInfo.Profile;
            AdditionalCommandLine = _sessionInfo.AdditionalCommandLine;

            // Get the group, if not --> get the first group (ascending), fallback --> default group 
            Group = string.IsNullOrEmpty(_sessionInfo.Group) ? (groups.Count > 0 ? groups.OrderBy(x => x).First() : Application.Current.Resources["String_Default"] as string) : _sessionInfo.Group;
            Tags = _sessionInfo.Tags;

            _groups = CollectionViewSource.GetDefaultView(groups);
            _groups.SortDescriptions.Add(new SortDescription());

            _isLoading = false;
        }

        private void HasSessionInfoChanged()
        {            
            if (ConnectionMode == ConnectionMode.Serial)
                SessionInfoChanged = (_sessionInfo.Name != Name) || (_sessionInfo.ConnectionMode != ConnectionMode) || (_sessionInfo.SerialLine != SerialLine) || (_sessionInfo.Baud != Baud) || (_sessionInfo.Username != Username) || (_sessionInfo.Profile != Profile) || (_sessionInfo.AdditionalCommandLine != AdditionalCommandLine) || (_sessionInfo.Group != Group) || (_sessionInfo.Tags != Tags);
            else
                SessionInfoChanged = (_sessionInfo.Name != Name) || (_sessionInfo.ConnectionMode != ConnectionMode) || (_sessionInfo.Host != Host) || (_sessionInfo.Port != Port) || (_sessionInfo.Username != Username) || (_sessionInfo.Profile != Profile) || (_sessionInfo.AdditionalCommandLine != AdditionalCommandLine) || (_sessionInfo.Group != Group) || (_sessionInfo.Tags != Tags);

            Debug.WriteLine(SessionInfoChanged);
        }
    }
}