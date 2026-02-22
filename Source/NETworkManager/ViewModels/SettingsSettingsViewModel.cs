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
    /// <summary>
    /// Gets or sets the action to execute when the associated object is closed.
    /// </summary>    
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
    public bool IsLocationManagedByPolicy => !string.IsNullOrWhiteSpace(PolicyManager.Current?.Settings_FolderLocation);

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
    private bool _isDefaultLocation;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsSettingsViewModel" /> class and loads the current settings.
    /// </summary>    
    public SettingsSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Load view specific settings.
    /// </summary>
    private void LoadSettings()
    {
        Location = SettingsManager.GetSettingsFolderLocation();
        IsDefaultLocation = string.Equals(Location, SettingsManager.GetDefaultSettingsFolderLocation(), StringComparison.OrdinalIgnoreCase);
        IsDailyBackupEnabled = SettingsManager.Current.Settings_IsDailyBackupEnabled;
        MaximumNumberOfBackups = SettingsManager.Current.Settings_MaximumNumberOfBackups;
    }

    #endregion

    #region ICommands & Actions
    /// <summary>
    /// Gets the command that opens the location folder selection dialog.
    /// </summary>    
    public ICommand BrowseLocationFolderCommand => new RelayCommand(p => BrowseLocationFolderAction());

    /// <summary>
    /// Opens a dialog that allows the user to select a folder location and updates the Location property with the
    /// selected path if the user confirms the selection.
    /// </summary>    
    private void BrowseLocationFolderAction()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();

        if (Directory.Exists(Location))
            dialog.SelectedPath = Location;

        var dialogResult = dialog.ShowDialog();

        if (dialogResult == System.Windows.Forms.DialogResult.OK)
            Location = dialog.SelectedPath;
    }

    /// <summary>
    /// Gets the command that opens a location when executed.
    /// </summary>    
    public ICommand OpenLocationCommand => new RelayCommand(_ => OpenLocationAction());

    /// <summary>
    /// Opens the settings folder location in Windows Explorer.
    /// </summary>    
    private static void OpenLocationAction()
    {
        Process.Start("explorer.exe", SettingsManager.GetSettingsFolderLocation());
    }

    /// <summary>
    /// Gets the command that initiates the action to change the location.
    /// </summary>    
    public ICommand ChangeLocationCommand => new RelayCommand(_ => ChangeLocationAction().ConfigureAwait(false));

    /// <summary>
    /// Prompts the user to confirm and then changes the location of the settings folder.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ChangeLocationAction()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
             Strings.ChangeLocationQuestion,
             string.Format(Strings.ChangeSettingsLocationMessage, SettingsManager.GetSettingsFolderLocation(), Location),
             ChildWindowIcon.Question,
             Strings.Change);

        if (!result)
            return;

        // Save settings at the current location before changing it to prevent
        // unintended saves to the new location (e.g., triggered by background timer or the app close & restart).
        SettingsManager.Save();

        // Set new location
        LocalSettingsManager.Current.Settings_FolderLocation = Location;
        LocalSettingsManager.Save();

        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }

    /// <summary>
    /// Gets the command that restores the default location settings asynchronously.
    /// </summary>    
    public ICommand RestoreDefaultLocationCommand => new RelayCommand(_ => RestoreDefaultLocationActionAsync().ConfigureAwait(false));

    /// <summary>
    /// Restores the application's settings folder location to the default path after obtaining user confirmation.
    /// </summary>
    /// <remarks>This method prompts the user to confirm the restoration of the default settings location. If
    /// the user confirms, it saves the current settings, clears any custom location, and restarts the application to
    /// apply the changes. Use this method when you want to revert to the default settings folder and ensure all changes
    /// are properly saved and applied.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RestoreDefaultLocationActionAsync()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.RestoreDefaultLocationQuestion,
            string.Format(Strings.RestoreDefaultSettingsLocationMessage, SettingsManager.GetSettingsFolderLocation(), SettingsManager.GetDefaultSettingsFolderLocation()),
            ChildWindowIcon.Question,
            Strings.Restore);

        if (!result)
            return;

        // Save settings at the current location before changing it to prevent
        // unintended saves to the new location (e.g., triggered by background timer or the app close & restart).
        SettingsManager.Save();

        // Clear custom location to revert to default
        LocalSettingsManager.Current.Settings_FolderLocation = null;
        LocalSettingsManager.Save();

        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }

    /// <summary>
    /// Gets the command that resets the application settings to their default values.
    /// </summary>    
    public ICommand ResetSettingsCommand => new RelayCommand(_ => ResetSettingsAction().ConfigureAwait(false));

    /// <summary>
    /// Resets the application settings to their default values.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ResetSettingsAction()
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

    #region Methods
    /// <summary>
    /// Sets the location path based on the provided drag-and-drop input.
    /// </summary>
    /// <param name="path">The path to set as the location.</param>
    public void SetLocationPathFromDragDrop(string path)
    {
        Location = path;
    }       
    #endregion
}
