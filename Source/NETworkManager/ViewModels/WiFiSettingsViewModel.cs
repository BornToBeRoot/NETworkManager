using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the WiFi settings.
/// </summary>
public class WiFiSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    /// <summary>
    /// Gets or sets a value indicating whether a notification is shown when an automatic refresh found changes.
    /// </summary>
    public bool ShowNotificationPopup
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_ShowNotificationPopup = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a sound is played when an automatic refresh found changes.
    /// </summary>
    public bool NotificationSound
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_NotificationSound = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the seconds until the notification is closed.
    /// </summary>
    public int NotificationCloseTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WiFi_NotificationCloseTime = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="WiFiSettingsViewModel"/> class.
    /// </summary>
    public WiFiSettingsViewModel()
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
        ShowNotificationPopup = SettingsManager.Current.WiFi_ShowNotificationPopup;
        NotificationSound = SettingsManager.Current.WiFi_NotificationSound;
        NotificationCloseTime = SettingsManager.Current.WiFi_NotificationCloseTime;
    }

    #endregion
}
