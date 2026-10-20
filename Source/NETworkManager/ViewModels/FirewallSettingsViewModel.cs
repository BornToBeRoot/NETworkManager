using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Firewall settings.
/// </summary>
public class FirewallSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    /// <summary>
    /// Gets or sets whether to show all system firewall rules or only rules managed by NETworkManager.
    /// </summary>
    public bool ShowAllRules
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Firewall_ShowAllRules = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="FirewallSettingsViewModel"/> class.
    /// </summary>
    public FirewallSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings into the ViewModel.
    /// </summary>
    private void LoadSettings()
    {
        ShowAllRules = SettingsManager.Current.Firewall_ShowAllRules;
    }

    #endregion
}