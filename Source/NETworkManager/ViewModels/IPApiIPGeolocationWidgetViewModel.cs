using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class IPApiIPGeolocationWidgetViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(IPApiIPGeolocationWidgetViewModel));

    private bool _isRunning;

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

    private IPGeolocationResult _result;

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

    public IPApiIPGeolocationWidgetViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    public ICommand CheckViaHotkeyCommand => new RelayCommand(_ => CheckViaHotkeyAction());

    private void CheckViaHotkeyAction()
    {
        Check();
    }

    #endregion

    #region Methods

    public void Check()
    {
        CheckAsync().ConfigureAwait(false);
    }

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
        await Task.Delay(2000);

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