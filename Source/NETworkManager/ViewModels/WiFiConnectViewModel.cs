using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;
using NETworkManager.Models.Network;

namespace NETworkManager.ViewModels;

public class WiFiConnectViewModel : ViewModelBase
{
    /// <summary>
    /// Command which is called when the Connect button is clicked.
    /// </summary>
    public ICommand ConnectCommand { get; }

    /// <summary>
    /// Command which is called when the Connect WPS button is clicked.
    /// </summary>
    public ICommand ConnectWpsCommand { get; }

    /// <summary>
    /// Command which is called when the cancel button is clicked.
    /// </summary>
    public ICommand CancelCommand { get; }

    public readonly (WiFiAdapterInfo AdapterInfo, WiFiNetworkInfo NetworkInfo) Options;

    private WiFiConnectMode _connectMode;
    public WiFiConnectMode ConnectMode
    {
        get => _connectMode;
        set
        {
            if (value == _connectMode)
                return;

            _connectMode = value;
            OnPropertyChanged();
        }
    }

    private bool _isSsidRequired;
    public bool IsSsidRequired
    {
        get => _isSsidRequired;
        set
        {
            if (value == _isSsidRequired)
                return;

            _isSsidRequired = value;
            OnPropertyChanged();
        }
    }

    private string _ssid;
    public string Ssid
    {
        get => _ssid;
        set
        {
            if (value == _ssid)
                return;

            _ssid = value;
            OnPropertyChanged();
        }
    }

    private bool _connectAutomatically;
    public bool ConnectAutomatically
    {
        get => _connectAutomatically;
        set
        {
            if (value == _connectAutomatically)
                return;

            _connectAutomatically = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Private variable for <see cref="PreSharedKey"/>.
    /// </summary>
    private SecureString _preSharedKey = new();

    /// <summary>
    /// Pre-shared key as secure string.
    /// </summary>
    public SecureString PreSharedKey
    {
        get => _preSharedKey;
        set
        {
            if (value == _preSharedKey)
                return;

            _preSharedKey = value;

            ValidatePreSharedKey();

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Private variable for <see cref="IsPreSharedKeyEmpty"/>.
    /// </summary>
    private bool _isPreSharedKeyEmpty = true;

    /// <summary>
    /// Indicate if the Pre-shared-key field is empty.
    /// </summary>
    public bool IsPreSharedKeyEmpty
    {
        get => _isPreSharedKeyEmpty;
        set
        {
            if (value == _isPreSharedKeyEmpty)
                return;

            _isPreSharedKeyEmpty = value;
            OnPropertyChanged();
        }
    }

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

    /// <summary>
    /// Private variable for <see cref="Password"/>.
    /// </summary>
    private SecureString _password = new();

    /// <summary>
    /// Password as secure string.
    /// </summary>
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

    /// <summary>
    /// Private variable for <see cref="IsPasswordEmpty"/>.
    /// </summary>
    private bool _isPasswordEmpty = true;

    /// <summary>
    /// Indicate if the password field is empty.
    /// </summary>
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

    private bool _isWpsAvailable;
    public bool IsWpsAvailable
    {
        get => _isWpsAvailable;
        set
        {
            if (value == _isWpsAvailable)
                return;

            _isWpsAvailable = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Initalizes a new class <see cref="CredentialsPasswordViewModel"/> with <see cref="ConnectCommand" /> and <see cref="CancelCommand"/>.
    /// </summary>
    /// <param name="okCommand"><see cref="ConnectCommand"/> which is executed on OK click.</param>
    /// <param name="cancelHandler"><see cref="CancelCommand"/> which is executed on cancel click.</param>
    public WiFiConnectViewModel(Action<WiFiConnectViewModel> okCommand, Action<WiFiConnectViewModel> connectWpsCommand, Action<WiFiConnectViewModel> cancelHandler, (WiFiAdapterInfo AdapterInfo, WiFiNetworkInfo NetworkInfo) options, WiFiConnectMode connectMode)
    {
        ConnectCommand = new RelayCommand(p => okCommand(this));
        ConnectWpsCommand = new RelayCommand(p => connectWpsCommand(this));
        CancelCommand = new RelayCommand(p => cancelHandler(this));

        Options = options;
        ConnectMode = connectMode;

        // Get Ssid to connect to hidden network
        IsSsidRequired = Options.NetworkInfo.IsHidden;
    }

    /// <summary>
    /// Check if the Pre-shared key is valid.
    /// </summary>
    private void ValidatePreSharedKey() => IsPreSharedKeyEmpty = PreSharedKey == null || PreSharedKey.Length == 0;

    /// <summary>
    /// Check if the password is valid.
    /// </summary>
    private void ValidatePassword() => IsPasswordEmpty = Password == null || Password.Length == 0;
}
