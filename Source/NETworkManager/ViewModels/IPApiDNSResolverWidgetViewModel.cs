using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using NETworkManager.Controls;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the IP API DNS resolver widget.
/// </summary>
public class IPApiDNSResolverWidgetViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(IPApiDNSResolverWidgetViewModel));

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
    /// Gets the result of the DNS resolver check.
    /// </summary>
    public DNSResolverResult Result
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
    /// Initializes a new instance of the <see cref="IPApiDNSResolverWidgetViewModel"/> class.
    /// </summary>
    public IPApiDNSResolverWidgetViewModel()
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
    /// Gets the command to check the DNS resolver.
    /// </summary>
    public ICommand CheckCommand => new RelayCommand(_ => Check(), _ => !IsRunning);

    #endregion

    #region Methods

    /// <summary>
    /// Checks the DNS resolver (also called by the dashboard, e.g. after a network change).
    /// </summary>
    public void Check()
    {
        // Already checking, e.g. the network changed right after the widget was created
        if (IsRunning)
            return;

        _ = CheckAsync();
    }

    /// <summary>
    /// Checks the DNS resolver asynchronously.
    /// </summary>
    private async Task CheckAsync()
    {
        // Check is disabled via settings
        if (!SettingsManager.Current.Dashboard_CheckIPApiDNSResolver)
            return;

        IsRunning = true;
        IsStatusMessageDisplayed = false;
        Result = null;

        try
        {
            // Takes at least the refresh interval, so the reload animation is visible and the check can't be spammed -
            // the request runs at the same time
            var result = await TaskHelper.WithMinimumDurationAsync(
                () => ScreenshotData.IsEnabled
                    ? Task.FromResult(ScreenshotData.CreateDNSResolverResult())
                    : DNSResolverService.GetInstance().GetDNSResolverAsync(),
                GlobalStaticConfiguration.ApplicationUIRefreshInterval);

            if (result.HasError)
            {
                Log.Error($"edns.ip-api.com error: {result.ErrorMessage}, error code: {result.ErrorCode}");

                StatusMessage = WebRequestErrorMessage.Get(DNSResolverService.Host, DNSResolverService.RequestTimeout,
                    result.Error);
                StatusMessageLevel = StatusLevel.Error;
                IsStatusMessageDisplayed = true;
            }
            else
            {
                Result = result;
            }
        }
        finally
        {
            IsRunning = false;

            // Enable the refresh button (CanExecute is otherwise only evaluated on the next input)
            CommandManager.InvalidateRequerySuggested();
        }
    }

    #endregion
}