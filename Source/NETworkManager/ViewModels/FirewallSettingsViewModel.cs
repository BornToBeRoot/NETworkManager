using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class FirewallSettingsViewModel : ViewModelBase
{
    #region Variables
    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;
    #endregion

    #region Constructor, load settings
    /// <summary>
    /// Construct the view model and load settings.
    /// </summary>
    public FirewallSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();
        
        _isLoading = false;
    }

    /// <summary>
    /// Load the settings via <see cref="SettingsManager"/>.
    /// </summary>
    private void LoadSettings()
    {
        
    }
    #endregion
}
