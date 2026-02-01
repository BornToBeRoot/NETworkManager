using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
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
    /// Backing field for <see cref="IsRunning"/>.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the check is running.
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Result"/>.
    /// </summary>
    private IPGeolocationResult _result;

    /// <summary>
    /// Gets the result of the IP geolocation check.
    /// </summary>
    public IPGeolocationResult Result
    {
        get => _result;
        private set
        {
            if (value == _result)
                return;

            _result = value;
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
    /// Gets the command to check via hotkey.
    /// </summary>
    public ICommand CheckViaHotkeyCommand => new RelayCommand(_ => CheckViaHotkeyAction());

    /// <summary>
    /// Action to check via hotkey.
    /// </summary>
    private void CheckViaHotkeyAction()
    {
        Check();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks the IP geolocation.
    /// </summary>
    public void Check()
    {
        CheckAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Checks the IP geolocation asynchronously.
    /// </summary>
    private async Task CheckAsync()
    {
        // Check is disabled via settings
        if (!SettingsManager.Current.Dashboard_CheckIPApiIPGeolocation)
            return;

        // Don't check multiple times if already running
        if (IsRunning)
            return;

        IsRunning = true;
        Result = null;

        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)        
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        Result = await IPGeolocationService.GetInstance().GetIPGeolocationAsync();

        // Log error
        if (Result.HasError)
            Log.Error($"ip-api.com error: {Result.ErrorMessage}, error code: {Result.ErrorCode}");

        // Log rate limit
        if (Result.RateLimitIsReached)
            Log.Warn($"ip-api.com rate limit reached. Try again in {Result.RateLimitRemainingTime} seconds.");

        IsRunning = false;
    }

    #endregion
}