using System;
using System.Collections.Generic;
using System.ComponentModel;
using NETworkManager.Models.Network;
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

    /// <summary>
    /// Gets or sets a value indicating whether the network connection widget is shown on the dashboard.
    /// </summary>
    public bool ShowNetworkConnectionWidget
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_ShowNetworkConnectionWidget = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the internet latency widget is shown on the dashboard.
    /// </summary>
    public bool ShowInternetLatencyWidget
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_ShowInternetLatencyWidget = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the positions of the internet latency widget.
    /// </summary>
    public IEnumerable<InternetLatencyPosition> InternetLatencyPositions => Enum.GetValues<InternetLatencyPosition>();

    /// <summary>
    /// Gets or sets the position of the internet latency widget (next to the speed test or own row).
    /// </summary>
    public InternetLatencyPosition InternetLatencyPosition
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyPosition = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the internet latency widget pings its targets.
    /// </summary>
    public bool CheckInternetLatencyEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckInternetLatency = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the time in seconds between the reply of a ping and the next ping of a target.
    /// </summary>
    public int InternetLatencyInterval
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyInterval = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the name of the internet latency target 1.
    /// </summary>
    public string InternetLatencyTarget1Name
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyTarget1Name = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the host name or IP address of the internet latency target 1 (empty: not shown).
    /// </summary>
    public string InternetLatencyTarget1Host
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyTarget1Host = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the name of the internet latency target 2.
    /// </summary>
    public string InternetLatencyTarget2Name
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyTarget2Name = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the host name or IP address of the internet latency target 2 (empty: not shown).
    /// </summary>
    public string InternetLatencyTarget2Host
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyTarget2Host = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the name of the internet latency target 3.
    /// </summary>
    public string InternetLatencyTarget3Name
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyTarget3Name = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the host name or IP address of the internet latency target 3 (empty: not shown).
    /// </summary>
    public string InternetLatencyTarget3Host
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_InternetLatencyTarget3Host = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the speed test widget is shown on the dashboard.
    /// </summary>
    public bool ShowSpeedTestWidget
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_ShowSpeedTestWidget = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the IP geolocation widget is shown on the dashboard.
    /// </summary>
    public bool ShowIPGeolocationWidget
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_ShowIPGeolocationWidget = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the DNS resolver widget is shown on the dashboard.
    /// </summary>
    public bool ShowDNSResolverWidget
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_ShowDNSResolverWidget = value;

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

        // The checks can also be enabled in the widgets (the settings view is created once)
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.Dashboard_CheckIPApiIPGeolocation):
                CheckIPApiIPGeolocationEnabled = SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation;
                break;
            case nameof(SettingsInfo.Dashboard_CheckIPApiDNSResolver):
                CheckIPApiDNSResolverEnabled = SettingsManager.Current.Dashboard_CheckIPApiDNSResolver;
                break;
            case nameof(SettingsInfo.Dashboard_CheckInternetLatency):
                CheckInternetLatencyEnabled = SettingsManager.Current.Dashboard_CheckInternetLatency;
                break;
        }
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
        ShowNetworkConnectionWidget = SettingsManager.Current.Dashboard_ShowNetworkConnectionWidget;
        ShowInternetLatencyWidget = SettingsManager.Current.Dashboard_ShowInternetLatencyWidget;
        CheckInternetLatencyEnabled = SettingsManager.Current.Dashboard_CheckInternetLatency;
        InternetLatencyPosition = SettingsManager.Current.Dashboard_InternetLatencyPosition;
        InternetLatencyInterval = SettingsManager.Current.Dashboard_InternetLatencyInterval;
        InternetLatencyTarget1Name = SettingsManager.Current.Dashboard_InternetLatencyTarget1Name;
        InternetLatencyTarget1Host = SettingsManager.Current.Dashboard_InternetLatencyTarget1Host;
        InternetLatencyTarget2Name = SettingsManager.Current.Dashboard_InternetLatencyTarget2Name;
        InternetLatencyTarget2Host = SettingsManager.Current.Dashboard_InternetLatencyTarget2Host;
        InternetLatencyTarget3Name = SettingsManager.Current.Dashboard_InternetLatencyTarget3Name;
        InternetLatencyTarget3Host = SettingsManager.Current.Dashboard_InternetLatencyTarget3Host;
        ShowSpeedTestWidget = SettingsManager.Current.Dashboard_ShowSpeedTestWidget;
        ShowIPGeolocationWidget = SettingsManager.Current.Dashboard_ShowIPGeolocationWidget;
        ShowDNSResolverWidget = SettingsManager.Current.Dashboard_ShowDNSResolverWidget;
    }

    #endregion
}