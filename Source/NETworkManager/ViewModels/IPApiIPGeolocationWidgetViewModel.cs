using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the IP API IP geolocation widget.
/// </summary>
public class IPApiIPGeolocationWidgetViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(IPApiIPGeolocationWidgetViewModel));

    /// <summary>
    /// Gets or sets a value indicating whether the check is running.
    /// </summary>
    public bool IsRunning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the result of the IP geolocation check.
    /// </summary>
    public IPGeolocationResult Result
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Message of the last check, e.g. an error or the rate limit (shown below the header).
    /// </summary>
    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public StatusLevel StatusMessageLevel
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsStatusMessageDisplayed
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="IPApiIPGeolocationWidgetViewModel"/> class.
    /// </summary>
    public IPApiIPGeolocationWidgetViewModel()
    {
        LoadSettings();
        Check();
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to check the IP geolocation.
    /// </summary>
    public ICommand CheckCommand => new RelayCommand(_ => Check(), _ => !IsRunning);

    #endregion

    #region Methods

    /// <summary>
    /// Checks the IP geolocation.
    /// </summary>
    private void Check()
    {
        _ = CheckAsync();
    }

    /// <summary>
    /// Checks the IP geolocation asynchronously.
    /// </summary>
    private async Task CheckAsync()
    {
        // Check is disabled via settings
        if (!SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation)
            return;

        IsRunning = true;
        IsStatusMessageDisplayed = false;
        Result = null;

        // Takes at least the refresh interval, so the reload animation is visible and the check can't be spammed -
        // the request runs at the same time
        var result = await TaskHelper.WithMinimumDurationAsync(
            () => ScreenshotData.IsEnabled
                ? Task.FromResult(ScreenshotData.CreateIPGeolocationResult())
                : IPGeolocationService.GetInstance().GetIPGeolocationAsync(),
            GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        if (result.HasError)
        {
            Log.Error($"ip-api.com error: {result.ErrorMessage}, error code: {result.ErrorCode}");

            ShowStatusMessage(WebRequestErrorMessage.Get(IPGeolocationService.Host,
                IPGeolocationService.RequestTimeout, result.Error), StatusLevel.Error);
        }
        else if (result.RateLimitIsReached)
        {
            Log.Warn($"ip-api.com rate limit reached. Try again in {result.RateLimitRemainingTime} seconds.");

            ShowStatusMessage(Strings.IPApiRateLimitMessage, StatusLevel.Warning);
        }
        else
        {
            Result = result;
        }

        IsRunning = false;
    }

    private void ShowStatusMessage(string message, StatusLevel level)
    {
        StatusMessage = message;
        StatusMessageLevel = level;
        IsStatusMessageDisplayed = true;
    }

    #endregion
}