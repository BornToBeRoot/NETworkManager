using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the dashboard settings.
/// </summary>
public class DashboardSettingsViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Gets or sets the public IPv4 address.
    /// </summary>
    public string PublicIPv4Address
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_PublicIPv4Address = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the public IPv6 address.
    /// </summary>
    public string PublicIPv6Address
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_PublicIPv6Address = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether checking public IP address is enabled.
    /// </summary>
    public bool CheckPublicIPAddressEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckPublicIPAddress = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use a custom API for public IPv4 address.
    /// </summary>
    public bool UsePublicIPv4AddressCustomAPI
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the custom API URL for public IPv4 address.
    /// </summary>
    public string CustomPublicIPv4AddressAPI
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use a custom API for public IPv6 address.
    /// </summary>
    public bool UsePublicIPv6AddressCustomAPI
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the custom API URL for public IPv6 address.
    /// </summary>
    public string CustomPublicIPv6AddressAPI
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether checking IP geolocation is enabled.
    /// </summary>
    public bool CheckIPApiIPGeolocationEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether checking IP API DNS resolver is enabled.
    /// </summary>
    public bool CheckIPApiDNSResolverEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckIPApiDNSResolver = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardSettingsViewModel"/> class.
    /// </summary>
    public DashboardSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        PublicIPv4Address = SettingsManager.Current.Dashboard_PublicIPv4Address;
        PublicIPv6Address = SettingsManager.Current.Dashboard_PublicIPv6Address;
        CheckPublicIPAddressEnabled = SettingsManager.Current.Dashboard_CheckPublicIPAddress;
        UsePublicIPv4AddressCustomAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI;
        CustomPublicIPv4AddressAPI = SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI;
        UsePublicIPv6AddressCustomAPI = SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI;
        CustomPublicIPv6AddressAPI = SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI;
        CheckIPApiIPGeolocationEnabled = SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation;
        CheckIPApiDNSResolverEnabled = SettingsManager.Current.Dashboard_CheckIPApiDNSResolver;
    }

    #endregion
}