using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Security;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class RemoteDesktopConnectViewModel : ViewModelBase
{
    public ICommand ConnectCommand { get; }

    public ICommand CancelCommand { get; }

    private bool _connectAs;
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

            _host = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView HostHistoryView { get; }

    private bool _useCredentials;
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

    private string _domain;
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

    private SecureString _password = new();
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

    private bool _isPasswordEmpty = true;
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

    public RemoteDesktopConnectViewModel(Action<RemoteDesktopConnectViewModel> connectCommand, Action<RemoteDesktopConnectViewModel> cancelHandler,(string Name, string Host)? connectAsOptions = null)
    {
        ConnectCommand = new RelayCommand(p => connectCommand(this));
        CancelCommand = new RelayCommand(p => cancelHandler(this));

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

    /// <summary>
    /// Check if the passwords are valid.
    /// </summary>
    private void ValidatePassword() => IsPasswordEmpty = Password == null || Password.Length == 0;
}
