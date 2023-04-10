using NETworkManager.Utilities;
using System;
using System.Security;
using System.Windows.Input;
using NETworkManager.Models.Network;
using System.Threading.Tasks;

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

    /// <summary>
    /// Current WiFi adapter info and network info.
    /// </summary>
    public readonly (WiFiAdapterInfo AdapterInfo, WiFiNetworkInfo NetworkInfo) Options;

    /// <summary>
    /// Private variable for <see cref="ConnectMode"/>.
    /// </summary>
    private WiFiConnectMode _connectMode;

    /// <summary>
    /// WiFi connect mode like <see cref="WiFiConnectMode.Open"/>, <see cref="WiFiConnectMode.Wps"/> or <see cref="WiFiConnectMode.Eap"/>.
    /// </summary>
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

    /// <summary>
    /// Private variable for <see cref="IsSsidRequired"/>.
    /// </summary>
    private bool _isSsidRequired;

    /// <summary>
    /// Indicate if the SSID field is required. 
    /// This is the case for hidden networks.
    /// </summary>
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

    /// <summary>
    /// Private variable for <see cref="Ssid"/>.
    /// </summary>
    private string _ssid;

    /// <summary>
    /// SSID of the network.
    /// </summary>
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

    /// <summary>
    /// Private variable for <see cref="ConnectAutomatically"/>.
    /// </summary>
    private bool _connectAutomatically;

    /// <summary>
    /// Indicate if the network should be connected automatically.
    /// </summary>
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
    /// Pre-shared key as <see cref="SecureString"/>.
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

    /// <summary>
    /// Private variable for <see cref="UseCredentials"/>.
    /// </summary>
    private bool _useCredentials = true;

    /// <summary>
    /// Use credentials for EAP authentication.
    /// </summary>
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

    /// <summary>
    /// Private variable for <see cref="Username"/>.
    /// </summary>
    private string _username;

    /// <summary>
    /// Username for EAP authentication.
    /// </summary>
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

    /// <summary>
    /// Private variable for <see cref="Domain"/>.
    /// </summary>
    private string _domain;

    /// <summary>
    /// Domain for EAP authentication.
    /// </summary>
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
    /// Password as <see cref="SecureString"/> for EAP authentication.
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

    /// <summary>
    /// Private variable for <see cref="IsWpsAvailable"/>.
    /// </summary>
    private bool _isWpsChecking;

    /// <summary>
    /// Checking if WPS is available for the network.
    /// </summary>
    public bool IsWpsChecking
    {
        get => _isWpsChecking;
        set
        {
            if (value == _isWpsChecking)
                return;

            _isWpsChecking = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Private variable for <see cref="IsWpsAvailable"/>.
    /// </summary>
    private bool _isWpsAvailable;

    /// <summary>
    /// Indicate if WPS is available for the network.
    /// </summary>
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

    public async Task CheckWpsAsync()
    {
        // Only check if WPS is available for networks secured by Pre-shared key who are not hidden
        if (ConnectMode != WiFiConnectMode.Psk || Options.NetworkInfo.IsHidden)
            return;

        IsWpsChecking = true;
        await Task.Delay(1000); // Show animation ;)

        IsWpsAvailable = await WiFi.IsWpsAvailable(Options.AdapterInfo.WiFiAdapter, Options.NetworkInfo.AvailableNetwork);

        await Task.Delay(1000); // Show animation ;)
        IsWpsChecking = false;
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
