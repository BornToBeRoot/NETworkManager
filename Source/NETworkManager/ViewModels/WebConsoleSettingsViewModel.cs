using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class WebConsoleSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private bool _showAddressBar;

    public bool ShowAddressBar
    {
        get => _showAddressBar;
        set
        {
            if (value == _showAddressBar)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_ShowAddressBar = value;

            _showAddressBar = value;
            OnPropertyChanged();
        }
    }

    private bool _isStatusBarEnabled;

    public bool IsStatusBarEnabled
    {
        get => _isStatusBarEnabled;
        set
        {
            if (value == _isStatusBarEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_IsStatusBarEnabled = value;

            _isStatusBarEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _isPasswordSaveEnabled;

    public bool IsPasswordSaveEnabled
    {
        get => _isPasswordSaveEnabled;
        set 
        {
            if (value == _isPasswordSaveEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_IsPasswordSaveEnabled = value;

            _isPasswordSaveEnabled = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public WebConsoleSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ShowAddressBar = SettingsManager.Current.WebConsole_ShowAddressBar;
        IsStatusBarEnabled = SettingsManager.Current.WebConsole_IsStatusBarEnabled;
        IsPasswordSaveEnabled = SettingsManager.Current.WebConsole_IsPasswordSaveEnabled;
    }

    #endregion
}