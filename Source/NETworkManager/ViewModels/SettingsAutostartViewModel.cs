using System;
using System.Threading.Tasks;
using System.Windows;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class SettingsAutostartViewModel : ViewModelBase
{
    #region Constructor

    public SettingsAutostartViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    #endregion

    #region Load settings

    private void LoadSettings()
    {
        StartWithWindows = AutostartManager.IsEnabled;
        StartMinimizedInTray = SettingsManager.Current.Autostart_StartMinimizedInTray;
    }

    #endregion

    #region Methods

    private async Task EnableDisableAutostart(bool enable)
    {
        ConfiguringAutostart = true;

        try
        {
            if (enable)
                await AutostartManager.EnableAsync();
            else
                await AutostartManager.DisableAsync();

            // Make the user happy, let him see a reload animation (and he cannot spam the reload command)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                ex.Message, ChildWindowIcon.Error);
        }

        ConfiguringAutostart = false;
    }

    #endregion

    #region Variables
    private readonly bool _isLoading;

    private bool _startWithWindows;

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set
        {
            if (value == _startWithWindows)
                return;

            if (!_isLoading)
                EnableDisableAutostart(value).ConfigureAwait(true);

            _startWithWindows = value;
            OnPropertyChanged();
        }
    }

    private bool _configuringAutostart;

    public bool ConfiguringAutostart
    {
        get => _configuringAutostart;
        set
        {
            if (value == _configuringAutostart)
                return;

            _configuringAutostart = value;
            OnPropertyChanged();
        }
    }

    private bool _startMinimizedInTray;

    public bool StartMinimizedInTray
    {
        get => _startMinimizedInTray;
        set
        {
            if (value == _startMinimizedInTray)
                return;

            if (!_isLoading)
                SettingsManager.Current.Autostart_StartMinimizedInTray = value;

            _startMinimizedInTray = value;
            OnPropertyChanged();
        }
    }

    #endregion
}