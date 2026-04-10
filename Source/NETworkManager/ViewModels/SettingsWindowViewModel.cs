using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public bool MinimizeInsteadOfTerminating
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Window_MinimizeInsteadOfTerminating = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool MinimizeToTrayInsteadOfTaskbar
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Window_MinimizeToTrayInsteadOfTaskbar = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ConfirmClose
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Window_ConfirmClose = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool MultipleInstances
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Window_MultipleInstances = value;
                        
            field = value;
            OnPropertyChanged();
        }
    }

    public bool AlwaysShowIconInTray
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.TrayIcon_AlwaysShowIcon = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool SplashScreenEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SplashScreen_Enabled = value;

            
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    public SettingsWindowViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        MinimizeInsteadOfTerminating = SettingsManager.Current.Window_MinimizeInsteadOfTerminating;
        ConfirmClose = SettingsManager.Current.Window_ConfirmClose;
        MultipleInstances = SettingsManager.Current.Window_MultipleInstances;
        MinimizeToTrayInsteadOfTaskbar = SettingsManager.Current.Window_MinimizeToTrayInsteadOfTaskbar;
        AlwaysShowIconInTray = SettingsManager.Current.TrayIcon_AlwaysShowIcon;
        SplashScreenEnabled = SettingsManager.Current.SplashScreen_Enabled;
    }

    #endregion
}