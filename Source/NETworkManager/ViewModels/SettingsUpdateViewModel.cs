using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

public class SettingsUpdateViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public bool CheckForUpdatesAtStartup
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Update_CheckForUpdatesAtStartup = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets whether the "Check for updates at startup" setting is managed by system-wide policy.
    /// </summary>
    public bool IsUpdateCheckManagedByPolicy => PolicyManager.Current?.Update_CheckForUpdatesAtStartup.HasValue == true;

    public bool CheckForPreReleases
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Update_CheckForPreReleases = value;

            field = value;
            OnPropertyChanged();
        }
    }


    public bool EnableExperimentalFeatures
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Experimental_EnableExperimentalFeatures = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    public SettingsUpdateViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        // If policy is set, show the policy value; otherwise show the user's setting
        CheckForUpdatesAtStartup = PolicyManager.Current?.Update_CheckForUpdatesAtStartup
                                    ?? SettingsManager.Current.Update_CheckForUpdatesAtStartup;
        CheckForPreReleases = SettingsManager.Current.Update_CheckForPreReleases;
        EnableExperimentalFeatures = SettingsManager.Current.Experimental_EnableExperimentalFeatures;
    }

    #endregion
}