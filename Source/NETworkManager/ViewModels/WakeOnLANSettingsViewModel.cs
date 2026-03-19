using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class WakeOnLANSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public int DefaultPort
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WakeOnLAN_Port = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public WakeOnLANSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        DefaultPort = SettingsManager.Current.WakeOnLAN_Port;
    }

    #endregion
}