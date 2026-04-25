using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class TracerouteSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public int MaximumHops
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Traceroute_MaximumHops = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Traceroute_Timeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int Buffer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Traceroute_Buffer = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ResolveHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Traceroute_ResolveHostname = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool CheckIPApiIPGeolocation
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Traceroute_CheckIPApiIPGeolocation = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public TracerouteSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        MaximumHops = SettingsManager.Current.Traceroute_MaximumHops;
        Timeout = SettingsManager.Current.Traceroute_Timeout;
        Buffer = SettingsManager.Current.Traceroute_Buffer;
        ResolveHostname = SettingsManager.Current.Traceroute_ResolveHostname;
        CheckIPApiIPGeolocation = SettingsManager.Current.Traceroute_CheckIPApiIPGeolocation;
    }

    #endregion
}