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
    #region Variables

    public bool ConnectAs
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

    public string Name
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

    public ICollectionView HostHistoryView { get; }

    public bool UseCredentials
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

    public string Domain
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

    public bool IsPasswordEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString Password
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            ValidatePassword();

            OnPropertyChanged();
        }
    } = new();

    public bool AdminSession
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

    #endregion

    #region Constructor

    public RemoteDesktopConnectViewModel(Action<RemoteDesktopConnectViewModel> connectCommand,
        Action<RemoteDesktopConnectViewModel> cancelHandler, (string Name, string Host, bool AdminSession)? connectAsOptions = null)
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

            AdminSession = connectAsOptions.Value.AdminSession;
        }
    }

    #endregion

    #region Commands

    public ICommand ConnectCommand { get; }

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