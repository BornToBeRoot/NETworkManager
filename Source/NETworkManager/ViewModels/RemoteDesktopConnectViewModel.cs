using System;
using System.ComponentModel;
using System.Security;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class RemoteDesktopConnectViewModel : ViewModelBase
{
    private bool _connectAs;

    private string _domain;

    private string _host;

    private bool _isPasswordEmpty = true;

    private string _name;

    private SecureString _password = new();

    private bool _useCredentials;

    private string _username;

    public RemoteDesktopConnectViewModel(Action<RemoteDesktopConnectViewModel> connectCommand,
        Action<RemoteDesktopConnectViewModel> cancelHandler, (string Name, string Host)? connectAsOptions = null)
    {
        ConnectCommand = new RelayCommand(_ => connectCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        if (connectAsOptions == null)
        {
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.RemoteDesktop_HostHistory);
        }
        else
        {
            ConnectAs = true;

            UseCredentials = true;

            Name = connectAsOptions.Value.Name;
            Host = connectAsOptions.Value.Host;
        }
    }

    public ICommand ConnectCommand { get; }

    public ICommand CancelCommand { get; }

    public bool ConnectAs
    {
        get => _connectAs;
        set
        {
            if (value == _connectAs)
                return;

            _connectAs = value;
            OnPropertyChanged();
        }
    }

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

    public ICollectionView HostHistoryView { get; }

    public bool UseCredentials
    {
        get => _useCredentials;
        set
        {
            if (value == _useCredentials)
                return;

            _useCredentials = value;
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

    public string Domain
    {
        get => _domain;
        set
        {
            if (value == _domain)
                return;

            _domain = value;
            OnPropertyChanged();
        }
    }

    public SecureString Password
    {
        get => _password;
        set
        {
            if (value == _password)
                return;

            _password = value;

            ValidatePassword();

            OnPropertyChanged();
        }
    }

    public bool IsPasswordEmpty
    {
        get => _isPasswordEmpty;
        set
        {
            if (value == _isPasswordEmpty)
                return;

            _isPasswordEmpty = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Check if the passwords are valid.
    /// </summary>
    private void ValidatePassword()
    {
        IsPasswordEmpty = Password == null || Password.Length == 0;
    }
}