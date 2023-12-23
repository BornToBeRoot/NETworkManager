using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;

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

        settings.AffirmativeButtonText = Strings.Reset;
        settings.NegativeButtonText = Strings.Cancel;

        settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

        if (await _dialogCoordinator.ShowMessageAsync(this, Strings.ResetSettingsQuestion,
                Strings.SettingsAreResetAndApplicationWillBeRestartedMessage,
                MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
            return;

        // Init default settings
        SettingsManager.Initialize();

        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }

    #endregion
}