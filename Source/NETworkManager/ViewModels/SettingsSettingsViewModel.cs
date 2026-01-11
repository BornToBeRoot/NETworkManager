using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SettingsSettingsViewModel : ViewModelBase
{
    #region Variables
    public Action CloseAction { get; set; }

    private readonly bool _isLoading;

    private string _location;

    public string Location
    {
        get => _location;
        set
        {
            if (value == _location)
                return;

            _location = value;
            OnPropertyChanged();
        }
    }

    private bool _isDailyBackupEnabled;

    public bool IsDailyBackupEnabled
    {
        get => _isDailyBackupEnabled;
        set
        {
            if (value == _isDailyBackupEnabled)
                return;
     
            if (!_isLoading)
                SettingsManager.Current.Settings_IsDailyBackupEnabled = value;
            
            _isDailyBackupEnabled = value;
            OnPropertyChanged();
        }
    }

    private int _maximumNumberOfBackups;

    public int MaximumNumberOfBackups
    {
        get => _maximumNumberOfBackups;
        set
        {
            if (value == _maximumNumberOfBackups)
                return;

            if (!_isLoading)
                SettingsManager.Current.Settings_MaximumNumberOfBackups = value;

            _maximumNumberOfBackups = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    public SettingsSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        Location = SettingsManager.GetSettingsFolderLocation();
        IsDailyBackupEnabled = SettingsManager.Current.Settings_IsDailyBackupEnabled;
        MaximumNumberOfBackups = SettingsManager.Current.Settings_MaximumNumberOfBackups;
    }

    #endregion

    #region ICommands & Actions

    public ICommand OpenLocationCommand => new RelayCommand(_ => OpenLocationAction());

    private static void OpenLocationAction()
    {
        Process.Start("explorer.exe", SettingsManager.GetSettingsFolderLocation());
    }

    public ICommand ResetSettingsCommand => new RelayCommand(_ => ResetSettingsAction());

    private void ResetSettingsAction()
    {
        ResetSettings().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task ResetSettings()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.ResetSettingsQuestion,
            Strings.SettingsAreResetAndApplicationWillBeRestartedMessage,
            ChildWindowIcon.Question,
            Strings.Reset);

        if (!result)
            return;

        // Init default settings
        SettingsManager.Initialize();

        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }
    #endregion
}
