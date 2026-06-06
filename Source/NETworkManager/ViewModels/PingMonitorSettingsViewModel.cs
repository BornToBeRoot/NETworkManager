using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents the settings for the Ping Monitor.
/// </summary>
public class PingMonitorSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    /// <summary>
    /// Gets or sets the timeout in milliseconds.
    /// </summary>
    public int Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_Timeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the buffer size in bytes.
    /// </summary>
    public int Buffer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_Buffer = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the time to live (TTL).
    /// </summary>
    public int TTL
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_TTL = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Don't Fragment flag is set.
    /// </summary>
    public bool DontFragment
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_DontFragment = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the wait time between pings in milliseconds.
    /// </summary>
    public int WaitTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_WaitTime = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the host view is expanded.
    /// </summary>
    public bool ExpandHostView
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_ExpandHostView = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the chart time window in seconds.
    /// </summary>
    public int ChartTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_ChartTime = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a notification popup is shown on status change.
    /// </summary>
    public bool ShowNotificationPopup
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_ShowNotificationPopup = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a sound is played on status change.
    /// </summary>
    public bool NotificationSound
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_NotificationSound = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the number of consecutive successful pings required before an UP notification fires.
    /// </summary>
    public int NotificationSuccessThreshold
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_NotificationSuccessThreshold = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the number of consecutive failed pings required before a DOWN notification fires.
    /// </summary>
    public int NotificationFailureThreshold
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_NotificationFailureThreshold = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the time in seconds the notification popup is shown before it closes automatically.
    /// </summary>
    public int NotificationCloseTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_NotificationCloseTime = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="PingMonitorSettingsViewModel"/> class.
    /// </summary>
    public PingMonitorSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        Timeout = SettingsManager.Current.PingMonitor_Timeout;
        Buffer = SettingsManager.Current.PingMonitor_Buffer;
        TTL = SettingsManager.Current.PingMonitor_TTL;
        DontFragment = SettingsManager.Current.PingMonitor_DontFragment;
        WaitTime = SettingsManager.Current.PingMonitor_WaitTime;
        ExpandHostView = SettingsManager.Current.PingMonitor_ExpandHostView;
        ChartTime = SettingsManager.Current.PingMonitor_ChartTime;
        ShowNotificationPopup = SettingsManager.Current.PingMonitor_ShowNotificationPopup;
        NotificationSound = SettingsManager.Current.PingMonitor_NotificationSound;
        NotificationSuccessThreshold = SettingsManager.Current.PingMonitor_NotificationSuccessThreshold;
        NotificationFailureThreshold = SettingsManager.Current.PingMonitor_NotificationFailureThreshold;
        NotificationCloseTime = SettingsManager.Current.PingMonitor_NotificationCloseTime;
    }

    #endregion
}