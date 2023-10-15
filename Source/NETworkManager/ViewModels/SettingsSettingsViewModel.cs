using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SettingsSettingsViewModel : ViewModelBase
{
    #region Variables
    private readonly IDialogCoordinator _dialogCoordinator;

    public Action CloseAction { get; set; }

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
    #endregion

    #region Constructor, LoadSettings
    public SettingsSettingsViewModel(IDialogCoordinator instance)
    {
        _dialogCoordinator = instance;

        LoadSettings();
    }

    private void LoadSettings()
    {
        Location = SettingsManager.GetSettingsFolderLocation();
    }
    #endregion

    #region ICommands & Actions
    public ICommand OpenLocationCommand => new RelayCommand(_ => OpenLocationAction());

    private static void OpenLocationAction()
    {
        Process.Start("explorer.exe", SettingsManager.GetSettingsFolderLocation());
    }        

    public ICommand ResetSettingsCommand => new RelayCommand(_ => ResetSettingsAction());

    private async void ResetSettingsAction()
    {
        var settings = AppearanceManager.MetroDialog;

        settings.AffirmativeButtonText = Localization.Resources.Strings.Reset;
        settings.NegativeButtonText = Localization.Resources.Strings.Cancel;

        settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

        if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.ResetSettingsQuestion, Localization.Resources.Strings.SettingsAreResetAndApplicationWillBeRestartedMessage, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
            return;

        // Init default settings
        SettingsManager.Initialize();
        
        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }
    #endregion
}
