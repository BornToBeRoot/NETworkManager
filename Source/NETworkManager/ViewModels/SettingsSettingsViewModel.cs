using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
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

    public SettingsSettingsViewModel()
    {
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

    private void ResetSettingsAction()
    {
        ResetSettings().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private Task ResetSettings()
    {
        var childWindow = new OKCancelInfoMessageChildWindow();

        var childWindowViewModel = new OKCancelInfoMessageViewModel(_ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            // Init default settings
            SettingsManager.Initialize();

            // Restart the application
            (Application.Current.MainWindow as MainWindow)?.RestartApplication();
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            Strings.SettingsAreResetAndApplicationWillBeRestartedMessage,
            Strings.Reset,
            Strings.Cancel,
            ChildWindowIcon.Question
        );

        childWindow.Title = Strings.ResetSettingsQuestion;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }
    #endregion
}
