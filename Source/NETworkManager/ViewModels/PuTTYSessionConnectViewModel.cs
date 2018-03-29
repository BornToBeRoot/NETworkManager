using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.ViewModels
{
    public class PuTTYSessionConnectViewModel : ViewModelBase
    {
        private readonly ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get { return _connectCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
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
                OnPropertyChanged();
            }
        }

        private bool _useSSH; // Defaul is SSH
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

        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                if (value == _port)
                    return;

                _port = value;
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
                OnPropertyChanged();
            }
        }

        public ConnectionMode ConnectionMode { get; set; }

        private ICollectionView _hostHistoryView;
        public ICollectionView HostHistoryView
        {
            get { return _hostHistoryView; }
        }

        private ICollectionView _serialLineHistoryView;
        public ICollectionView SerialLineHistoryView
        {
            get { return _serialLineHistoryView; }
        }

        private ICollectionView _portHistoryView;
        public ICollectionView PortHistoryView
        {
            get { return _portHistoryView; }
        }

        private ICollectionView _baudHistoryView;
        public ICollectionView BaudHistoryView
        {
            get { return _baudHistoryView; }
        }

        private ICollectionView _usernameHistoryView;
        public ICollectionView UsernameHistoryView
        {
            get { return _usernameHistoryView; }
        }

        private ICollectionView _profileHistoryView;
        public ICollectionView ProfileHistoryView
        {
            get { return _profileHistoryView; }
        }

        public PuTTYSessionConnectViewModel(Action<PuTTYSessionConnectViewModel> connectCommand, Action<PuTTYSessionConnectViewModel> cancelHandler)
        {
            _connectCommand = new RelayCommand(p => connectCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_HostHistory);
            _serialLineHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_SerialLineHistory);
            _portHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_PortHistory);
            _baudHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_BaudHistory);
            _usernameHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_UsernameHistory);
            _profileHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_ProfileHistory);

            // SSH is default...
            UseSSH = true;
        }
    }
}
