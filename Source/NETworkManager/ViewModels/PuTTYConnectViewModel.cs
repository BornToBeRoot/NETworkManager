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
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseSSH
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                Port = SettingsManager.Current.PuTTY_SSHPort;
                ConnectionMode = ConnectionMode.SSH;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseTelnet
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                Port = SettingsManager.Current.PuTTY_TelnetPort;
                ConnectionMode = ConnectionMode.Telnet;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseSerial
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                Baud = SettingsManager.Current.PuTTY_BaudRate;
                ConnectionMode = ConnectionMode.Serial;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseRlogin
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                Port = SettingsManager.Current.PuTTY_RloginPort;
                ConnectionMode = ConnectionMode.Rlogin;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseRAW
    {
        get;
        set
        {
            if (value == field)
                return;

            if (value)
            {
                Port = SettingsManager.Current.PuTTY_RawPort;
                ConnectionMode = ConnectionMode.RAW;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public string Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string SerialLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int Baud
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Username
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string PrivateKeyFile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Profile
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string AdditionalCommandLine
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
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