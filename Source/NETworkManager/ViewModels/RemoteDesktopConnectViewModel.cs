using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Security;
using System.Windows.Data;

namespace NETworkManager.ViewModels;

public class RemoteDesktopConnectViewModel : ConnectDialogViewModelBase<RemoteDesktopConnectViewModel>
{
    #region Variables
    private bool _connectAs;

    public bool ConnectAs
    {
        get => _connectAs;
        set => SetProperty(ref _connectAs, value);
    }

    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _host;

    public string Host
    {
        get => _host;
        set => SetProperty(ref _host, value);
    }

    public ICollectionView HostHistoryView { get; }

    private bool _useCredentials;

    public bool UseCredentials
    {
        get => _useCredentials;
        set => SetProperty(ref _useCredentials, value);
    }

    private string _domain;

    public string Domain
    {
        get => _domain;
        set => SetProperty(ref _domain, value);
    }
        set
        {
            if (value == _domain)
                return;

            _domain = value;
            OnPropertyChanged();
        }
    }

    private string _username;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private bool _isPasswordEmpty = true;

    public bool IsPasswordEmpty
    {
        get => _isPasswordEmpty;
        set => SetProperty(ref _isPasswordEmpty, value);
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

    private bool _adminSession;

    public bool AdminSession
    {
        get => _adminSession;
        set => SetProperty(ref _adminSession, value);
    }
    #endregion

    #region Constructor

    public RemoteDesktopConnectViewModel(Action<RemoteDesktopConnectViewModel> connectCommand,
        Action<RemoteDesktopConnectViewModel> cancelHandler, (string Name, string Host, bool AdminSession)? connectAsOptions = null)
        : base(connectCommand, cancelHandler)
    {
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

            AdminSession = connectAsOptions.Value.AdminSession;
        }
    }

    #endregion

    #region Commands
    // Commands are inherited from ConnectDialogViewModelBase

    public ICommand CancelCommand { get; }
    #endregion

    #region Methods

    /// <summary>
    ///     Check if the passwords are valid.
    /// </summary>
    private void ValidatePassword()
    {
        IsPasswordEmpty = Password == null || Password.Length == 0;
    }
    #endregion
}