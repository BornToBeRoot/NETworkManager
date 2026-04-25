using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class SettingsNetworkViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public bool UseCustomDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_UseCustomDNSServer = value;

            field = value;
            OnPropertyChanged();
        }
    }


    public string CustomDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_CustomDNSServer = value.Replace(" ", "");

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ResolveHostnamePreferIPv4
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4 = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ResolveHostnamePreferIPv6
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