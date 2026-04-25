using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using NETworkManager.Settings;
using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class WiFiConnectViewModel : ViewModelBase
{
    /// <summary>
    ///     Current WiFi adapter info and network info.
    /// </summary>
    public readonly (WiFiAdapterInfo AdapterInfo, WiFiNetworkInfo NetworkInfo) Options;

    /// <summary>
    ///     Initialize a new class <see cref="CredentialsPasswordViewModel" /> with <see cref="ConnectCommand" /> and
    ///     <see cref="CancelCommand" />.
    /// </summary>
    /// <param name="okCommand"><see cref="ConnectCommand" /> which is executed on OK click.</param>
    /// <param name="connectWpsCommand"><see cref="ConnectWpsCommand" /> which is executed on WPS click.</param>
    /// <param name="cancelHandler"><see cref="CancelCommand" /> which is executed on cancel click.</param>
    /// <param name="options">Current WiFi adapter info and network info.</param>
    /// <param name="connectMode">
    ///     WiFi connect mode like <see cref="WiFiConnectMode.Open" />,
    ///     <see cref="WiFiConnectMode.Psk" /> or <see cref="WiFiConnectMode.Eap" />.
    /// </param>
    public WiFiConnectViewModel(Action<WiFiConnectViewModel> okCommand, Action<WiFiConnectViewModel> connectWpsCommand,
        Action<WiFiConnectViewModel> cancelHandler, (WiFiAdapterInfo AdapterInfo, WiFiNetworkInfo NetworkInfo) options,
        WiFiConnectMode connectMode)
    {
        ConnectCommand = new RelayCommand(_ => okCommand(this));
        ConnectWpsCommand = new RelayCommand(_ => connectWpsCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        Options = options;
        ConnectMode = connectMode;

        // Get Ssid to connect to hidden network
        IsSsidRequired = Options.NetworkInfo.IsHidden;
    }

    /// <summary>
    ///     Command which is called when the Connect button is clicked.
    /// </summary>
    public ICommand ConnectCommand { get; }

    /// <summary>
    ///     Command which is called when the Connect WPS button is clicked.
    /// </summary>
    public ICommand ConnectWpsCommand { get; }

    /// <summary>
    ///     Command which is called when the cancel button is clicked.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    ///     WiFi connect mode like <see cref="WiFiConnectMode.Open" />, <see cref="WiFiConnectMode.Psk" /> or
    ///     <see cref="WiFiConnectMode.Eap" />.
    /// </summary>
    public WiFiConnectMode ConnectMode
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

    /// <summary>
    ///     Indicate if the SSID field is required.
    ///     This is the case for hidden networks.
    /// </summary>
    public bool IsSsidRequired
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

    /// <summary>
    ///     SSID of the network.
    /// </summary>
    public string Ssid
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

    /// <summary>
    ///     Indicate if the network should be connected automatically.
    /// </summary>
    public bool ConnectAutomatically
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

    /// <summary>
    ///     Pre-shared key as <see cref="SecureString" />.
    /// </summary>
    public SecureString PreSharedKey
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;

            ValidatePreSharedKey();

            OnPropertyChanged();
        }
    } = new();

    /// <summary>
    ///     Indicate if the Pre-shared-key field is empty.
    /// </summary>
    public bool IsPreSharedKeyEmpty
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

    /// <summary>
    ///     Use credentials for EAP authentication.
    /// </summary>
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
    } = true;

    /// <summary>
    ///     Username for EAP authentication.
    /// </summary>
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

    /// <summary>
    ///     Domain for EAP authentication.
    /// </summary>
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

    /// <summary>
    ///     Password as <see cref="SecureString" /> for EAP authentication.
    /// </summary>
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

    /// <summary>
    ///     Indicate if the password field is empty.
    /// </summary>
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

    /// <summary>
    ///     Checking if WPS is available for the network.
    /// </summary>
    public bool IsWpsChecking
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

    /// <summary>
    ///     Indicate if WPS is available for the network.
    /// </summary>
    public bool IsWpsAvailable
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

    public async Task CheckWpsAsync()
    {
        // Only check if WPS is available for networks secured by Pre-shared key who are not hidden
        if (ConnectMode != WiFiConnectMode.Psk || Options.NetworkInfo.IsHidden)
            return;

        IsWpsChecking = true;
        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval / 2);

        IsWpsAvailable =
            await WiFi.IsWpsAvailable(Options.AdapterInfo.WiFiAdapter, Options.NetworkInfo.AvailableNetwork);

        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)        
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval / 2);
        IsWpsChecking = false;
    }

    /// <summary>
    ///     Check if the Pre-shared key is valid.
    /// </summary>
    private void ValidatePreSharedKey()
    {
        IsPreSharedKeyEmpty = PreSharedKey == null || PreSharedKey.Length == 0;
    }

    /// <summary>
    ///     Check if the password is valid.
    /// </summary>
    private void ValidatePassword()
    {
        IsPasswordEmpty = Password == null || Password.Length == 0;
    }
}
