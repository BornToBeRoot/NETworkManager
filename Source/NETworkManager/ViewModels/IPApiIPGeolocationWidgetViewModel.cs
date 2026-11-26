using System;
using System.Globalization;
using System.Text;
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
    /// Gets the time of the last successful check, <c>null</c> before it and while checking (the values belong to it).
    /// </summary>
    public DateTime? LastCheckTime
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

    /// <summary>
    /// Gets the command to copy all values as text, e.g. for a support request.
    /// </summary>
    public ICommand CopyAllCommand => new RelayCommand(_ => CopyAllAction(), _ => !IsRunning && Result?.Info != null);

    /// <summary>
    /// Gets the command to enable the IP geolocation check (setting <c>Dashboard_CheckIPApiIPGeolocation</c>, off by default: ip-api.com receives
    /// the IP address) and to check right away.
    /// </summary>
    public ICommand EnableCheckCommand => new RelayCommand(_ => EnableCheckAction());

    private void CopyAllAction()
    {
        var info = Result.Info;
        var text = new StringBuilder();

        text.AppendLine(Strings.Location);

        foreach (var (label, value) in new[]
                 {
                     (Strings.Continent, info.ContinentWithCode),
                     (Strings.Country, info.CountryWithCode),
                     (Strings.Region, info.RegionWithCode),
                     (Strings.City, info.City),
                     (Strings.District, info.District),
                     (Strings.ZipCode, info.Zip),
                     (Strings.Latitude, FormatCoordinate(info.Lat)),
                     (Strings.Longitude, FormatCoordinate(info.Lon)),
                     (Strings.Timezone, info.Timezone),
                     (Strings.Offset, info.UtcOffset),
                     (Strings.Currency, info.Currency)
                 })
            text.AppendLine($"{label}: {FormatValue(value)}");

        text.AppendLine();
        text.AppendLine(Strings.Network);

        foreach (var (label, value) in new[]
                 {
                     (Strings.IPAddress, info.Query),
                     (Strings.PTRRecord, info.Reverse),
                     (Strings.ISP, info.Isp),
                     (Strings.Organization, info.Org),
                     (Strings.ASN, info.As),
                     (Strings.ASName, info.Asname),
                     (Strings.Hosting, info.Hosting ? Strings.Yes : Strings.No),
                     (Strings.Proxy, info.Proxy ? Strings.Yes : Strings.No),
                     (Strings.Mobile, info.Mobile ? Strings.Yes : Strings.No)
                 })
            text.AppendLine($"{label}: {FormatValue(value)}");

        ClipboardHelper.SetClipboard(text.ToString().TrimEnd());
    }

    // Like the widget: "-/-" for an empty value (and 0 for a coordinate, which ip-api.com returns without a location)
    private static string FormatValue(string value) => string.IsNullOrEmpty(value) ? "-/-" : value;

    private static string FormatCoordinate(double value) =>
        value == 0 ? "-/-" : value.ToString(CultureInfo.CurrentCulture);

    private void EnableCheckAction()
    {
        SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation = true;

        Check();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks the IP geolocation (also called by the dashboard, e.g. after a network change).
    /// </summary>
    public void Check()
    {
        // Already checking, e.g. the network changed right after the widget was created
        if (IsRunning)
            return;

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
        LastCheckTime = null;

        try
        {
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

                // Like the IP Geolocation tool and the traceroute
                ShowStatusMessage(
                    $"{IPGeolocationService.Host} {string.Format(Strings.RateLimitReachedTryAgainInXSeconds, result.RateLimitRemainingTime)}",
                    StatusLevel.Warning);
            }
            else
            {
                Result = result;
                LastCheckTime = DateTime.Now;
            }
        }
        finally
        {
            IsRunning = false;

            // Enable the refresh button (CanExecute is otherwise only evaluated on the next input)
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void ShowStatusMessage(string message, StatusLevel level)
    {
        StatusMessage = message;
        StatusMessageLevel = level;
        IsStatusMessageDisplayed = true;
    }

    #endregion
}