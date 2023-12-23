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
    }

    #endregion
}