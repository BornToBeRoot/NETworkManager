using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.ViewModels
{
    public class PuTTYConnectViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }

        public ICommand CancelCommand { get; }

        public int DefaultSSHPort => SettingsManager.Current.PuTTY_SSHPort;
        public int DefaultTelnetPort => SettingsManager.Current.PuTTY_TelnetPort;
        public int DefaultBaudRate => SettingsManager.Current.PuTTY_BaudRate;
        public int DefaultRloginPort => SettingsManager.Current.PuTTY_RloginPort;
        public int DefaultRaw => SettingsManager.Current.PuTTY_DefaultRaw;

        public ConnectionMode ConnectionMode { get; set; }

        private bool _useSSH;
        public bool UseSSH
        {
            get => _useSSH;
            set
            {
                if (value == _useSSH)
                    return;

                if (value)
                {
                    Port = DefaultSSHPort;
                    ConnectionMode = ConnectionMode.SSH;
                }

                _useSSH = value;
                OnPropertyChanged();
            }
        }

        private bool _useTelnet;
        public bool UseTelnet
        {
            get => _useTelnet;
            set
            {
                if (value == _useTelnet)
                    return;

                if (value)
                {
                    Port = DefaultTelnetPort;
                    ConnectionMode = ConnectionMode.Telnet;
                }

                _useTelnet = value;
                OnPropertyChanged();
            }
        }

        private bool _useSerial;
        public bool UseSerial
        {
            get => _useSerial;
            set
            {
                if (value == _useSerial)
                    return;

                if (value)
                {
                    Baud = DefaultBaudRate;
                    ConnectionMode = ConnectionMode.Serial;
                }

                _useSerial = value;
                OnPropertyChanged();
            }
        }

        private bool _useRlogin;
        public bool UseRlogin
        {
            get => _useRlogin;
            set
            {
                if (value == _useRlogin)
                    return;

                if (value)
                {
                    Port = DefaultRloginPort;
                    ConnectionMode = ConnectionMode.Rlogin;
                }

                _useRlogin = value;
                OnPropertyChanged();
            }
        }

        private bool _useRAW;
        public bool UseRAW
        {
            get => _useRAW;
            set
            {
                if (value == _useRAW)
                    return;

                if (value)
                {
                    Port = DefaultRaw;
                    ConnectionMode = ConnectionMode.RAW;
                }

                _useRAW = value;
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
                OnPropertyChanged();
            }
        }

        private string _serialLine;
        public string SerialLine
        {
            get => _serialLine;
            set
            {
                if (value == _serialLine)
                    return;

                _serialLine = value;
                OnPropertyChanged();
            }
        }

        private int _port;
        public int Port
        {
            get => _port;
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
            get => _baud;
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
            get => _username;
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
            get => _profile;
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
            get => _additionalCommandLine;
            set
            {
                if (value == _additionalCommandLine)
                    return;

                _additionalCommandLine = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView HostHistoryView { get; }

        public ICollectionView SerialLineHistoryView { get; }

        public ICollectionView PortHistoryView { get; }

        public ICollectionView BaudHistoryView { get; }

        public ICollectionView UsernameHistoryView { get; }

        public ICollectionView ProfileHistoryView { get; }

        public PuTTYConnectViewModel(Action<PuTTYConnectViewModel> connectCommand, Action<PuTTYConnectViewModel> cancelHandler, string host = null)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            if (!string.IsNullOrEmpty(host))
                Host = host;

            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_HostHistory);
            SerialLineHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_SerialLineHistory);
            PortHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_PortHistory);
            BaudHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_BaudHistory);
            UsernameHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_UsernameHistory);
            ProfileHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_ProfileHistory);

            LoadSettings();
        }

        private void LoadSettings()
        {

            ConnectionMode = SettingsManager.Current.PuTTY_DefaultConnectionMode;

            switch (ConnectionMode)
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

            Username = SettingsManager.Current.PuTTY_Username;
            Profile = SettingsManager.Current.PuTTY_Profile;
            SerialLine = SettingsManager.Current.PuTTY_SerialLine;
            AdditionalCommandLine = SettingsManager.Current.PuTTY_AdditionalCommandLine;
        }
    }
}
