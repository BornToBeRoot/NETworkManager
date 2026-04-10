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

    public bool StartWithWindows
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                EnableDisableAutostart(value).ConfigureAwait(true);

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ConfiguringAutostart
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

    public bool StartMinimizedInTray
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Autostart_StartMinimizedInTray = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion
}