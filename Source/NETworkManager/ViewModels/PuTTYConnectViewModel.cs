using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.PuTTY;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class PuTTYConnectViewModel : ViewModelBase
{
    private string _additionalCommandLine;

    private int _baud;

    private ConnectionMode _connectionMode;

    private string _host;

    private int _port;

    private string _privateKeyFile;


    private string _profile;

    private string _serialLine;

    private bool _useRAW;

    private bool _useRlogin;

    private string _username;

    private bool _useSerial;

    private bool _useSSH;

    private bool _useTelnet;

    public PuTTYConnectViewModel(Action<PuTTYConnectViewModel> connectCommand,
        Action<PuTTYConnectViewModel> cancelHandler, string host = null)
    {
        ConnectCommand = new RelayCommand(_ => connectCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        if (!string.IsNullOrEmpty(host))
            Host = host;

        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_HostHistory);
        SerialLineHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_SerialLineHistory);
        PortHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_PortHistory);
        BaudHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_BaudHistory);
        UsernameHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_UsernameHistory);
        PrivateKeyFileHistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_PrivateKeyFileHistory);
        ProfileHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_ProfileHistory);

        LoadSettings();
    }

    public ICommand ConnectCommand { get; }

    public ICommand CancelCommand { get; }

    public ConnectionMode ConnectionMode
    {
        get => _connectionMode;
        set
        {
            if (value == _connectionMode)
                return;

            _connectionMode = value;
            OnPropertyChanged();
        }
    }

    public bool UseSSH
    {
        get => _useSSH;
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

    public bool UseTelnet
    {
        get => _useTelnet;
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

    public bool UseSerial
    {
        get => _useSerial;
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

    public bool UseRlogin
    {
        get => _useRlogin;
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

    public bool UseRAW
    {
        get => _useRAW;
        set
        {
            if (value == _useRAW)
                return;

            if (value)
            {
                Port = SettingsManager.Current.PuTTY_RawPort;
                ConnectionMode = ConnectionMode.RAW;
            }

            _useRAW = value;
            OnPropertyChanged();
        }
    }

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

    public string PrivateKeyFile
    {
        get => _privateKeyFile;
        set
        {
            if (value == _privateKeyFile)
                return;

            _privateKeyFile = value;
            OnPropertyChanged();
        }
    }

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

    public ICollectionView PrivateKeyFileHistoryView { get; set; }

    public ICollectionView ProfileHistoryView { get; }

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
        PrivateKeyFile = SettingsManager.Current.PuTTY_PrivateKeyFile;
        Profile = SettingsManager.Current.PuTTY_Profile;
        SerialLine = SettingsManager.Current.PuTTY_SerialLine;
        AdditionalCommandLine = SettingsManager.Current.PuTTY_AdditionalCommandLine;
    }
}