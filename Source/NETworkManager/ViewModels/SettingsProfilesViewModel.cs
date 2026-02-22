using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SettingsProfilesViewModel : ViewModelBase
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

            if (!_isLoading)
                IsLocationChanged = !string.Equals(value, ProfileManager.GetProfilesFolderLocation(), StringComparison.OrdinalIgnoreCase);

            _location = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Indicates whether the profiles location is managed by a system-wide policy.
    /// </summary>
    public bool IsLocationManagedByPolicy => !string.IsNullOrWhiteSpace(PolicyManager.Current?.Profiles_FolderLocation);

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
    /// Indicates whether the current location is the default profiles folder location.
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
    /// Private field of <see cref="ProfileFiles" /> property.
    /// </summary>
    private readonly ICollectionView _profileFiles;

    /// <summary>
    /// Gets the collection view of profile files.
    /// </summary>    
    public ICollectionView ProfileFiles
    {
        get => _profileFiles;
        private init
        {
            if (value == _profileFiles)
                return;

            _profileFiles = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Private field of <see cref="SelectedProfileFile" /> property.
    /// </summary>

    private ProfileFileInfo _selectedProfileFile;

    /// <summary>
    /// Gets or sets the currently selected profile file information.
    /// </summary>    
    public ProfileFileInfo SelectedProfileFile
    {
        get => _selectedProfileFile;
        set
        {
            if (Equals(value, _selectedProfileFile))
                return;

            _selectedProfileFile = value;
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
                SettingsManager.Current.Profiles_IsDailyBackupEnabled = value;

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
                SettingsManager.Current.Profiles_MaximumNumberOfBackups = value;

            _maximumNumberOfBackups = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Constructor, LoadSettings

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsProfilesViewModel" /> class and loads the current profile files.
    /// </summary>    
    public SettingsProfilesViewModel()
    {
        _isLoading = true;

        ProfileFiles = new CollectionViewSource { Source = ProfileManager.ProfileFiles }.View;
        ProfileFiles.SortDescriptions.Add(
            new SortDescription(nameof(ProfileFileInfo.Name), ListSortDirection.Ascending));

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>().FirstOrDefault();

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Load view specific settings.
    /// </summary>
    private void LoadSettings()
    {
        Location = ProfileManager.GetProfilesFolderLocation();
        IsDefaultLocation = string.Equals(Location, ProfileManager.GetDefaultProfilesFolderLocation(), StringComparison.OrdinalIgnoreCase);
        IsDailyBackupEnabled = SettingsManager.Current.Profiles_IsDailyBackupEnabled;
        MaximumNumberOfBackups = SettingsManager.Current.Profiles_MaximumNumberOfBackups;
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
    /// Gets the command that initiates the action to change the location.
    /// </summary>    
    public ICommand OpenLocationCommand => new RelayCommand(_ => OpenLocationAction());

    private static void OpenLocationAction()
    {
        Process.Start("explorer.exe", ProfileManager.GetProfilesFolderLocation());
    }    

    /// <summary>
    /// Gets the command that initiates the action to change the location.
    /// </summary>    
    public ICommand ChangeLocationCommand => new RelayCommand(_ => ChangeLocationAction().ConfigureAwait(false));

    /// <summary>
    /// Prompts the user to confirm and then changes the location of the profiles folder.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ChangeLocationAction()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
             Strings.ChangeLocationQuestion,
             string.Format(Strings.ChangeProfilesLocationMessage, ProfileManager.GetProfilesFolderLocation(), Location),
             ChildWindowIcon.Question,
             Strings.Change);

        if (!result)
            return;

        // Set new location in SettingsInfo
        SettingsManager.Current.Profiles_FolderLocation = Location;

        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }

    /// <summary>
    /// Gets the command that restores the default location.
    /// </summary>    
    public ICommand RestoreDefaultLocationCommand => new RelayCommand(_ => RestoreDefaultLocationActionAsync().ConfigureAwait(false));

    /// <summary>
    /// Restores the profiles folder location to the default path after obtaining user confirmation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RestoreDefaultLocationActionAsync()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.RestoreDefaultLocationQuestion,
            string.Format(Strings.RestoreDefaultProfilesLocationMessage, ProfileManager.GetProfilesFolderLocation(), ProfileManager.GetDefaultProfilesFolderLocation()),
            ChildWindowIcon.Question,
            Strings.Restore);

        if (!result)
            return;

        // Clear custom location to revert to default
        SettingsManager.Current.Profiles_FolderLocation = null;
        
        // Restart the application
        (Application.Current.MainWindow as MainWindow)?.RestartApplication();
    }

    public ICommand AddProfileFileCommand => new RelayCommand(async _ => await AddProfileFileAction().ConfigureAwait(false));

    private async Task AddProfileFileAction()
    {
        var profileName = string.Empty;

        var childWindow = new ProfileFileChildWindow();

        var childWindowViewModel = new ProfileFileViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            profileName = instance.Name;

            ProfileManager.CreateEmptyProfileFile(instance.Name);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.AddProfileFile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);

        // Re-select the profile file
        if (string.IsNullOrEmpty(profileName))
            return;

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>()
            .FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));

        // Ask the user if they want to enable encryption for the new profile file
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.EnableEncryptionQuestion,
            Strings.EnableEncryptionForProfileFileMessage);

        if (result)
            EnableEncryptionAction();
    }

    public ICommand EditProfileFileCommand => new RelayCommand(async _ => await EditProfileFileAction().ConfigureAwait(false));

    private async Task EditProfileFileAction()
    {
        var profileName = string.Empty;

        var childWindow = new ProfileFileChildWindow();

        var childWindowViewModel = new ProfileFileViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            profileName = instance.Name;

            ProfileManager.RenameProfileFile(SelectedProfileFile, instance.Name);
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, SelectedProfileFile);

        childWindow.Title = Strings.EditProfileFile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);

        // Re-select the profile file
        if (string.IsNullOrEmpty(profileName))
            return;

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>()
            .FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
    }

    public ICommand DeleteProfileFileCommand =>
        new RelayCommand(async _ => await DeleteProfileFileAction().ConfigureAwait(false), DeleteProfileFile_CanExecute);

    private bool DeleteProfileFile_CanExecute(object obj)
    {
        return ProfileFiles.Cast<ProfileFileInfo>().Count() > 1;
    }

    private async Task DeleteProfileFileAction()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.DeleteProfileFile,
            string.Format(Strings.DeleteProfileFileXMessage, SelectedProfileFile.Name),
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        ProfileManager.DeleteProfileFile(SelectedProfileFile);

        // Select the first profile file
        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>().FirstOrDefault();
    }

    public ICommand EnableEncryptionCommand => new RelayCommand(async _ => await EnableEncryptionAction().ConfigureAwait(false));

    private async Task EnableEncryptionAction()
    {
        // Show encryption disclaimer
        if (!await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
             Strings.Disclaimer,
             Strings.ProfileEncryptionDisclaimer))
            return;


        var profileFile = SelectedProfileFile.Name;

        var childWindow = new CredentialsSetPasswordChildWindow();

        var childWindowViewModel = new CredentialsSetPasswordViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
            try
            {
                ProfileManager.EnableEncryption(SelectedProfileFile, instance.Password);
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow,
                    Strings.EncryptionError,
                    $"{Strings.EncryptionErrorMessage}\n\n{ex.Message}",
                    ChildWindowIcon.Error).ConfigureAwait(false);
            }
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.SetMasterPassword;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);

        // Re-select the profile file
        if (string.IsNullOrEmpty(profileFile))
            return;

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>()
            .FirstOrDefault(p => p.Name.Equals(profileFile, StringComparison.OrdinalIgnoreCase));
    }

    public ICommand ChangeMasterPasswordCommand => new RelayCommand(async _ => await ChangeMasterPasswordAction().ConfigureAwait(false));

    private async Task ChangeMasterPasswordAction()
    {
        var profileName = SelectedProfileFile.Name;

        var childWindow = new CredentialsChangePasswordChildWindow();

        var childWindowViewModel = new CredentialsChangePasswordViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ProfileManager.ChangeMasterPassword(SelectedProfileFile, instance.Password, instance.NewPassword);
            }
            catch (CryptographicException)
            {
                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow,
                    Strings.WrongPassword,
                    Strings.WrongPasswordDecryptionFailedMessage,
                    ChildWindowIcon.Error).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow,
                    Strings.DecryptionError,
                    $"{Strings.DecryptionErrorMessage}\n\n{ex.Message}",
                    ChildWindowIcon.Error).ConfigureAwait(false);
            }
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.ChangeMasterPassword;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);

        // Re-select the profile file
        if (string.IsNullOrEmpty(profileName))
            return;

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>()
            .FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
    }

    public ICommand DisableEncryptionCommand => new RelayCommand(async _ => await DisableEncryptionAction().ConfigureAwait(false));

    private async Task DisableEncryptionAction()
    {
        var profileName = SelectedProfileFile.Name;

        var childWindow = new CredentialsPasswordChildWindow();

        var childWindowViewModel = new CredentialsPasswordViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ProfileManager.DisableEncryption(SelectedProfileFile, instance.Password);
            }
            catch (CryptographicException)
            {
                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow,
                    Strings.WrongPassword,
                    Strings.WrongPasswordDecryptionFailedMessage,
                    ChildWindowIcon.Error).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow,
                    Strings.DecryptionError,
                    $"{Strings.DecryptionErrorMessage}\n\n{ex.Message}",
                    ChildWindowIcon.Error).ConfigureAwait(false);
            }

        }, _ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            });

        childWindow.Title = Strings.MasterPassword;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);

        // Re-select the profile file
        if (string.IsNullOrEmpty(profileName))
            return;

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>()
            .FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
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
