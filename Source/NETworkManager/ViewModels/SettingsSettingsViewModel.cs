using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SettingsSettingsViewModel : ViewModelBase
{
    #region Variables
    public Action CloseAction { get; set; }

    /// <summary>
    /// Indicates whether the settings are currently being loaded to prevent triggering change events during initialization.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Private field of <see cref="Location" /> property.
    /// </summary>
    private string _location;

    /// <summary>
    /// Gets or sets the file system path to the settings location.
    /// </summary>    
    public string Location
    {
        get => _location;
        set
        {
            if (value == _location)
                return;

            if (!_isLoading)
                IsLocationChanged = !string.Equals(value, SettingsManager.GetSettingsFolderLocation(), StringComparison.OrdinalIgnoreCase);

            _location = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Indicates whether the settings location is managed by a system-wide policy.
    /// </summary>
    public bool IsLocationManagedByPolicy => !string.IsNullOrWhiteSpace(PolicyManager.Current?.SettingsFolderLocation);

    /// <summary>
    /// Private field of <see cref="IsLocationChanged" /> property.
    /// </summary>
    private bool _isLocationChanged;

    /// <summary>
    /// Gets or sets a value indicating whether the location has changed.
    /// </summary>
    public bool IsLocationChanged
    {
        get => _isLocationChanged;
        set
        {
            if (value == _isLocationChanged)
                return;

            _isLocationChanged = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Private field of <see cref="IsDefaultLocation" /> property.
    /// </summary>
    public bool _isDefaultLocation;

    /// <summary>
    /// Indicates whether the current location is the default settings folder location.
    /// </summary>
    public bool IsDefaultLocation
    {
        get => _isDefaultLocation;
        set
        {
            if (value == _isDefaultLocation)
                return;

            _isDefaultLocation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Private field of <see cref="IsDailyBackupEnabled" /> property.
    /// </summary>
    private bool _isDailyBackupEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether daily backups are enabled.
    /// </summary>
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

    /// <summary>
    /// Private field of <see cref="MaximumNumberOfBackups" /> property.
    /// </summary>
    private int _maximumNumberOfBackups;

    /// <summary>
    /// Gets or sets the maximum number of backups to keep.
    /// </summary>
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
        IsDefaultLocation = string.Equals(Location, SettingsManager.GetDefaultSettingsFolderLocation(), StringComparison.OrdinalIgnoreCase);

        Debug.WriteLine(Location);
        Debug.WriteLine(SettingsManager.GetDefaultSettingsFolderLocation());

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
    public ICommand BrowseLocationFolderCommand => new RelayCommand(p => BrowseLocationFolderAction());

    private void BrowseLocationFolderAction()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();

        if (Directory.Exists(Location))
            dialog.SelectedPath = Location;

        var dialogResult = dialog.ShowDialog();

        if (dialogResult == System.Windows.Forms.DialogResult.OK)
            Location = dialog.SelectedPath;
    }

    public void SetLocationPathFromDragDrop(string path)
    {
        Location = path;
    }

    public ICommand ChangeLocationCommand => new RelayCommand(_ => ChangeLocationAction());

    private async Task ChangeLocationAction()
    {
        /*
       var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.ChangeSettingsLocationQuestion,
            Strings.SettingsLocationWillBeChangedAndApplicationWillBeRestartedMessage,
            ChildWindowIcon.Question,
            Strings.Apply);

        LocalSettingsManager.Save();

        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
        */
    }

    public ICommand RestoreDefaultLocationCommand => new RelayCommand(_ => RestoreDefaultLocationActionAsync().ConfigureAwait(false));

    private async Task RestoreDefaultLocationActionAsync()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.RestoreDefaultLocationQuestion,
            string.Format(Strings.RestoreDefaultLocationSettingsMessage, SettingsManager.GetSettingsFolderLocation(),SettingsManager.GetDefaultSettingsFolderLocation()),
            ChildWindowIcon.Question,
            Strings.Restore);

        if (!result)
            return;

        LocalSettingsManager.Current.SettingsFolderLocation = null;

        LocalSettingsManager.Save();

        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }

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
