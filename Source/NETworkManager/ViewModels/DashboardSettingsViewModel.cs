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
    /// Gets or sets a value indicating whether the latency widget is shown on the dashboard.
    /// </summary>
    public bool ShowLatencyWidget
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_ShowLatencyWidget = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the positions of the latency widget.
    /// </summary>
    public IEnumerable<LatencyPosition> LatencyPositions => Enum.GetValues<LatencyPosition>();

    /// <summary>
    /// Gets or sets the position of the latency widget (next to the speed test or own row).
    /// </summary>
    public LatencyPosition LatencyPosition
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyPosition = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the latency widget pings its targets.
    /// </summary>
    public bool CheckLatencyEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_CheckLatency = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the time in seconds between the reply of a ping and the next ping of a target.
    /// </summary>
    public int LatencyInterval
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyInterval = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the name of the latency target 1.
    /// </summary>
    public string LatencyTarget1Name
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyTarget1Name = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the host name or IP address of the latency target 1 (empty: not shown).
    /// </summary>
    public string LatencyTarget1Host
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyTarget1Host = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the name of the latency target 2.
    /// </summary>
    public string LatencyTarget2Name
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyTarget2Name = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the host name or IP address of the latency target 2 (empty: not shown).
    /// </summary>
    public string LatencyTarget2Host
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyTarget2Host = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the name of the latency target 3.
    /// </summary>
    public string LatencyTarget3Name
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyTarget3Name = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the host name or IP address of the latency target 3 (empty: not shown).
    /// </summary>
    public string LatencyTarget3Host
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Dashboard_LatencyTarget3Host = value;

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
            case nameof(SettingsInfo.Dashboard_CheckLatency):
                CheckLatencyEnabled = SettingsManager.Current.Dashboard_CheckLatency;
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
        ShowLatencyWidget = SettingsManager.Current.Dashboard_ShowLatencyWidget;
        CheckLatencyEnabled = SettingsManager.Current.Dashboard_CheckLatency;
        LatencyPosition = SettingsManager.Current.Dashboard_LatencyPosition;
        LatencyInterval = SettingsManager.Current.Dashboard_LatencyInterval;
        LatencyTarget1Name = SettingsManager.Current.Dashboard_LatencyTarget1Name;
        LatencyTarget1Host = SettingsManager.Current.Dashboard_LatencyTarget1Host;
        LatencyTarget2Name = SettingsManager.Current.Dashboard_LatencyTarget2Name;
        LatencyTarget2Host = SettingsManager.Current.Dashboard_LatencyTarget2Host;
        LatencyTarget3Name = SettingsManager.Current.Dashboard_LatencyTarget3Name;
        LatencyTarget3Host = SettingsManager.Current.Dashboard_LatencyTarget3Host;
        ShowSpeedTestWidget = SettingsManager.Current.Dashboard_ShowSpeedTestWidget;
        ShowIPGeolocationWidget = SettingsManager.Current.Dashboard_ShowIPGeolocationWidget;
        ShowDNSResolverWidget = SettingsManager.Current.Dashboard_ShowDNSResolverWidget;
    }

    #endregion
}