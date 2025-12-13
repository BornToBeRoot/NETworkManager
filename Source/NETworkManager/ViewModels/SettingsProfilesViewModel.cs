using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
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

    private readonly ICollectionView _profileFiles;

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

    private ProfileFileInfo _selectedProfileFile;

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

    #endregion

    #region Constructor, LoadSettings

    public SettingsProfilesViewModel(IDialogCoordinator instance)
    {
        _dialogCoordinator = instance;

        ProfileFiles = new CollectionViewSource { Source = ProfileManager.ProfileFiles }.View;
        ProfileFiles.SortDescriptions.Add(
            new SortDescription(nameof(ProfileFileInfo.Name), ListSortDirection.Ascending));

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>().FirstOrDefault();

        LoadSettings();
    }

    private void LoadSettings()
    {
        Location = ProfileManager.GetProfilesFolderLocation();
    }

    #endregion

    #region ICommands & Actions

    public ICommand OpenLocationCommand => new RelayCommand(_ => OpenLocationAction());

    private static void OpenLocationAction()
    {
        Process.Start("explorer.exe", ProfileManager.GetProfilesFolderLocation());
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

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);

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

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);

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

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);

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

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);

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

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);

        // Re-select the profile file
        if (string.IsNullOrEmpty(profileName))
            return;

        SelectedProfileFile = ProfileFiles.Cast<ProfileFileInfo>()
            .FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}
