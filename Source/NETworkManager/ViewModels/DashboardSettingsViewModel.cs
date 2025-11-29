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
    /// Backing field for <see cref="PublicIPv4Address"/>.
    /// </summary>
    private string _publicIPv4Address;

    /// <summary>
    /// Gets or sets the public IPv4 address.
    /// </summary>
    public string PublicIPv4Address
    {
        get => _publicIPv4Address;
        set
        {
            if (value == _publicIPv4Address)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_PublicIPv4Address = value;

            _publicIPv4Address = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="PublicIPv6Address"/>.
    /// </summary>
    private string _publicIPv6Address;

    /// <summary>
    /// Gets or sets the public IPv6 address.
    /// </summary>
    public string PublicIPv6Address
    {
        get => _publicIPv6Address;
        set
        {
            if (value == _publicIPv6Address)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_PublicIPv6Address = value;

            _publicIPv6Address = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="CheckPublicIPAddressEnabled"/>.
    /// </summary>
    private bool _checkPublicIPAddressEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether checking public IP address is enabled.
    /// </summary>
    public bool CheckPublicIPAddressEnabled
    {
        get => _checkPublicIPAddressEnabled;
        set
        {
            if (value == _checkPublicIPAddressEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckPublicIPAddress = value;

            _checkPublicIPAddressEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="UsePublicIPv4AddressCustomAPI"/>.
    /// </summary>
    private bool _usePublicIPv4AddressCustomAPI;

    /// <summary>
    /// Gets or sets a value indicating whether to use a custom API for public IPv4 address.
    /// </summary>
    public bool UsePublicIPv4AddressCustomAPI
    {
        get => _usePublicIPv4AddressCustomAPI;
        set
        {
            if (value == _usePublicIPv4AddressCustomAPI)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_UseCustomPublicIPv4AddressAPI = value;

            _usePublicIPv4AddressCustomAPI = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="CustomPublicIPv4AddressAPI"/>.
    /// </summary>
    private string _customPublicIPv4AddressAPI;

    /// <summary>
    /// Gets or sets the custom API URL for public IPv4 address.
    /// </summary>
    public string CustomPublicIPv4AddressAPI
    {
        get => _customPublicIPv4AddressAPI;
        set
        {
            if (value == _customPublicIPv4AddressAPI)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CustomPublicIPv4AddressAPI = value;

            _customPublicIPv4AddressAPI = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="UsePublicIPv6AddressCustomAPI"/>.
    /// </summary>
    private bool _usePublicIPv6AddressCustomAPI;

    /// <summary>
    /// Gets or sets a value indicating whether to use a custom API for public IPv6 address.
    /// </summary>
    public bool UsePublicIPv6AddressCustomAPI
    {
        get => _usePublicIPv6AddressCustomAPI;
        set
        {
            if (value == _usePublicIPv6AddressCustomAPI)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_UseCustomPublicIPv6AddressAPI = value;

            _usePublicIPv6AddressCustomAPI = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="CustomPublicIPv6AddressAPI"/>.
    /// </summary>
    private string _customPublicIPv6AddressAPI;

    /// <summary>
    /// Gets or sets the custom API URL for public IPv6 address.
    /// </summary>
    public string CustomPublicIPv6AddressAPI
    {
        get => _customPublicIPv6AddressAPI;
        set
        {
            if (value == _customPublicIPv6AddressAPI)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CustomPublicIPv6AddressAPI = value;

            _customPublicIPv6AddressAPI = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="CheckIPApiIPGeolocationEnabled"/>.
    /// </summary>
    private bool _checkIPApiIPGeolocationEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether checking IP geolocation is enabled.
    /// </summary>
    public bool CheckIPApiIPGeolocationEnabled
    {
        get => _checkIPApiIPGeolocationEnabled;
        set
        {
            if (value == _checkIPApiIPGeolocationEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation = value;

            _checkIPApiIPGeolocationEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="CheckIPApiDNSResolverEnabled"/>.
    /// </summary>
    private bool _checkIPApiDNSResolverEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether checking IP API DNS resolver is enabled.
    /// </summary>
    public bool CheckIPApiDNSResolverEnabled
    {
        get => _checkIPApiDNSResolverEnabled;
        set
        {
            if (value == _checkIPApiDNSResolverEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckIPApiDNSResolver = value;

            _checkIPApiDNSResolverEnabled = value;
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