using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents the settings for the Ping Monitor.
/// </summary>
public class PingMonitorSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private int _timeout;

    /// <summary>
    /// Gets or sets the timeout in milliseconds.
    /// </summary>
    public int Timeout
    {
        get => _timeout;
        set
        {
            if (value == _timeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_Timeout = value;

            _timeout = value;
            OnPropertyChanged();
        }
    }

    private int _buffer;

    /// <summary>
    /// Gets or sets the buffer size in bytes.
    /// </summary>
    public int Buffer
    {
        get => _buffer;
        set
        {
            if (value == _buffer)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_Buffer = value;

            _buffer = value;
            OnPropertyChanged();
        }
    }

    private int _ttl;

    /// <summary>
    /// Gets or sets the time to live (TTL).
    /// </summary>
    public int TTL
    {
        get => _ttl;
        set
        {
            if (value == _ttl)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_TTL = value;

            _ttl = value;
            OnPropertyChanged();
        }
    }

    private bool _dontFragment;

    /// <summary>
    /// Gets or sets a value indicating whether the Don't Fragment flag is set.
    /// </summary>
    public bool DontFragment
    {
        get => _dontFragment;
        set
        {
            if (value == _dontFragment)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_DontFragment = value;

            _dontFragment = value;
            OnPropertyChanged();
        }
    }

    private int _waitTime;

    /// <summary>
    /// Gets or sets the wait time between pings in milliseconds.
    /// </summary>
    public int WaitTime
    {
        get => _waitTime;
        set
        {
            if (value == _waitTime)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_WaitTime = value;

            _waitTime = value;
            OnPropertyChanged();
        }
    }

    private bool _expandHostView;

    /// <summary>
    /// Gets or sets a value indicating whether the host view is expanded.
    /// </summary>
    public bool ExpandHostView
    {
        get => _expandHostView;
        set
        {
            if (value == _expandHostView)
                return;

            if (!_isLoading)
                SettingsManager.Current.PingMonitor_ExpandHostView = value;

            _expandHostView = value;
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
    }

    #endregion
}