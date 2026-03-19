using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class SettingsStatusViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public bool ShowWindowOnNetworkChange
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Status_ShowWindowOnNetworkChange = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public int WindowCloseTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Status_WindowCloseTime = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    public SettingsStatusViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ShowWindowOnNetworkChange = SettingsManager.Current.Status_ShowWindowOnNetworkChange;
        WindowCloseTime = SettingsManager.Current.Status_WindowCloseTime;
    }

    #endregion
}