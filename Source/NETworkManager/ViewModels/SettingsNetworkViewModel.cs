using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class SettingsNetworkViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private bool _useCustomDNSServer;

    public bool UseCustomDNSServer
    {
        get => _useCustomDNSServer;
        set
        {
            if (value == _useCustomDNSServer)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_UseCustomDNSServer = value;

            _useCustomDNSServer = value;
            OnPropertyChanged();
        }
    }


    private string _customDNSServer;

    public string CustomDNSServer
    {
        get => _customDNSServer;
        set
        {
            if (value == _customDNSServer)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_CustomDNSServer = value.Replace(" ", "");

            _customDNSServer = value;
            OnPropertyChanged();
        }
    }

    private bool _resolveHostnamePreferIPv4;

    public bool ResolveHostnamePreferIPv4
    {
        get => _resolveHostnamePreferIPv4;
        set
        {
            if (value == _resolveHostnamePreferIPv4)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4 = value;

            _resolveHostnamePreferIPv4 = value;
            OnPropertyChanged();
        }
    }

    private bool _resolveHostnamePreferIPv6;

    public bool ResolveHostnamePreferIPv6
    {
        get => _resolveHostnamePreferIPv6;
        set
        {
            if (value == _resolveHostnamePreferIPv6)
                return;

            _resolveHostnamePreferIPv6 = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    public SettingsNetworkViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        UseCustomDNSServer = SettingsManager.Current.Network_UseCustomDNSServer;

        if (SettingsManager.Current.Network_CustomDNSServer != null)
            CustomDNSServer = string.Join("; ", SettingsManager.Current.Network_CustomDNSServer);

        if (SettingsManager.Current.Network_ResolveHostnamePreferIPv4)
            ResolveHostnamePreferIPv4 = true;
        else
            ResolveHostnamePreferIPv6 = true;
    }

    #endregion
}