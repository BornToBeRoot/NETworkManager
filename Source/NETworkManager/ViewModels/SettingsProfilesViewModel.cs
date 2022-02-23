using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class SettingsProfilesViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public Action CloseAction { get; set; }
        public bool IsPortable => ConfigurationManager.Current.IsPortable;

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

        private bool _movingFiles;
        public bool MovingFiles
        {
            get => _movingFiles;
            set
            {
                if (value == _movingFiles)
                    return;

                _movingFiles = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _profileFiles;
        public ICollectionView ProfileFiles
        {
            get => _profileFiles;
            set
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
            ProfileFiles.SortDescriptions.Add(new SortDescription(nameof(ProfileFileInfo.Name), ListSortDirection.Ascending));

            LoadSettings();
        }

        private void LoadSettings()
        {
            Location = ProfileManager.GetProfilesLocation();
        }
        #endregion

        #region ICommands & Actions
        public ICommand BrowseLocationFolderCommand => new RelayCommand(p => BrowseLocationFolderAction());

        private void BrowseLocationFolderAction()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (Directory.Exists(Location))
                    dialog.SelectedPath = Location;

                var dialogResult = dialog.ShowDialog();

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                    Location = dialog.SelectedPath;
            }
        }

        public ICommand OpenLocationCommand => new RelayCommand(p => OpenLocationAction());

        private static void OpenLocationAction()
        {
            Process.Start("explorer.exe", ProfileManager.GetProfilesLocation());
        }

        public ICommand ChangeLocationCommand => new RelayCommand(p => ChangeLocationAction());

        private async Task ChangeLocationAction()
        {
            MovingFiles = true;

            // Check if a profile file with the same name is in the new folder
            var containsDuplicatedFiles = false;

            var files = Directory.GetFiles(Location);

            var containsOtherFiles = files.Any();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                foreach (var profileFile in ProfileManager.ProfileFiles)
                {
                    if (fileName == Path.GetFileName(profileFile.Path))
                    {
                        containsDuplicatedFiles = true;
                        break;
                    }
                }

                if (containsDuplicatedFiles)
                    break;
            }

            /*  
             *  -1  Cancel
             *  0   Move and overwrite
             *  1   Move and merge
             *  2   User other path (don't move/overwrite files)
             */
            int decision = -1;


            // Ask to overwrite or just set the new location
            if (containsDuplicatedFiles)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.Overwrite;
                settings.NegativeButtonText = Localization.Resources.Strings.Cancel;
                settings.FirstAuxiliaryButtonText = Localization.Resources.Strings.Merge;
                settings.SecondAuxiliaryButtonText = Localization.Resources.Strings.UseOtherFolder;
                
                var result = await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Overwrite, Localization.Resources.Strings.OverwriteProfilesInDestinationFolderMessage + Environment.NewLine + Environment.NewLine + Localization.Resources.Strings.ApplicationWillBeRestartedAfterwards, MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, AppearanceManager.MetroDialog);

                switch (result)
                {
                    case MessageDialogResult.Affirmative:
                        decision = 0;
                        break;
                    case MessageDialogResult.FirstAuxiliary:
                        decision = 1;
                        break;
                    case MessageDialogResult.SecondAuxiliary:
                        decision = 2;
                        break;
                }
            }
            else if (containsOtherFiles)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.Merge;
                settings.NegativeButtonText = Localization.Resources.Strings.Cancel;
                
                if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Merge, Localization.Resources.Strings.MergeProfileFilesInDestinationFolderMessage + Environment.NewLine + Environment.NewLine + Localization.Resources.Strings.ApplicationWillBeRestartedAfterwards, MessageDialogStyle.AffirmativeAndNegative, AppearanceManager.MetroDialog) == MessageDialogResult.Affirmative)
                    decision = 1;
            }
            else
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;
                settings.NegativeButtonText = Localization.Resources.Strings.Cancel;

                if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Move, Localization.Resources.Strings.ProfileFilesWillBeMovedMessage + Environment.NewLine + Environment.NewLine + Localization.Resources.Strings.ApplicationWillBeRestartedAfterwards, MessageDialogStyle.AffirmativeAndNegative, AppearanceManager.MetroDialog) == MessageDialogResult.Affirmative)
                    decision = 1;
            }

            // Canceled
            if (decision == -1)
            {
                MovingFiles = false;

                return;
            }

            // Move (overwrite or merge)
            if (decision == 0 || decision == 1)
            {
                try
                {
                    var overwrite = decision == 0;

                    await ProfileManager.MoveProfilesAsync(Location, overwrite);

                    // Show the user some awesome animation to indicate we are working on it :)
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;

                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
                }
            }
                                    
            // Set new location
            SettingsManager.Current.Profiles_CustomProfilesLocation = Location;
                        
            // Restart the application
            ConfigurationManager.Current.SoftRestart = true;
            CloseAction();

            MovingFiles = false;
        }

        public ICommand RestoreDefaultProfilesLocationCommand => new RelayCommand(p => RestoreDefaultProfilesLocationAction());

        private void RestoreDefaultProfilesLocationAction()
        {
            Location = ProfileManager.GetDefaultProfilesLocation();
        }

        public ICommand AddProfileFileCommand => new RelayCommand(p => AddProfileFileAction());

        private async Task AddProfileFileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddProfileFile
            };

            var profileFileViewModel = new ProfileFileViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.CreateEmptyProfileFile(instance.Name);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new ProfileFileDialog
            {
                DataContext = profileFileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditProfileFileCommand => new RelayCommand(p => EditProfileFileAction());

        private async Task EditProfileFileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.EditProfileFile
            };

            var profileFileViewModel = new ProfileFileViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.RenameProfileFile(SelectedProfileFile, instance.Name);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, SelectedProfileFile);

            customDialog.Content = new ProfileFileDialog
            {
                DataContext = profileFileViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteProfileFileCommand => new RelayCommand(p => DeleteProfileFileAction(), DeleteProfileFile_CanExecute);

        private bool DeleteProfileFile_CanExecute(object obj)
        {
            return ProfileFiles.Cast<ProfileFileInfo>().Count() > 1;
        }

        private async Task DeleteProfileFileAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.DeleteProfileFile
            };

            var confirmDeleteViewModel = new ConfirmDeleteViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                ProfileManager.DeleteProfileFile(SelectedProfileFile);
            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Localization.Resources.Strings.DeleteProfileFileMessage);

            customDialog.Content = new ConfirmDeleteDialog
            {
                DataContext = confirmDeleteViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EnableEncryptionCommand => new RelayCommand(p => EnableEncryptionAction());

        private async Task EnableEncryptionAction()
        {
            var settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Localization.Resources.Strings.OK;
            settings.NegativeButtonText = Localization.Resources.Strings.Cancel;
            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Disclaimer, Localization.Resources.Strings.ProfileEncryptionDisclaimer, MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
            {
                var customDialog = new CustomDialog
                {
                    Title = Localization.Resources.Strings.SetMasterPassword
                };

                var credentialsSetPasswordViewModel = new CredentialsSetPasswordViewModel(async instance =>
                {
                    await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                    try
                    {
                        ProfileManager.EnableEncryption(SelectedProfileFile, instance.Password);
                    }
                    catch (Exception ex)
                    {
                        var settings = AppearanceManager.MetroDialog;
                        settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                        await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.EncryptionError, $"{Localization.Resources.Strings.EncryptionErrorMessage}\n\n{ex.Message}", MessageDialogStyle.Affirmative, settings);
                    }
                }, async instance =>
                {
                    await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                });

                customDialog.Content = new CredentialsSetPasswordDialog
                {
                    DataContext = credentialsSetPasswordViewModel
                };

                await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
            }
        }

        public ICommand ChangeMasterPasswordCommand => new RelayCommand(p => ChangeMasterPasswordAction());

        private async Task ChangeMasterPasswordAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.ChangeMasterPassword
            };

            var credentialsPasswordViewModel = new CredentialsChangePasswordViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ProfileManager.ChangeMasterPassword(SelectedProfileFile, instance.Password, instance.NewPassword);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.WrongPassword, Localization.Resources.Strings.WrongPasswordDecryptionFailedMessage, MessageDialogStyle.Affirmative, settings);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.DecryptionError, $"{Localization.Resources.Strings.DecryptionErrorMessage}\n\n{ex.Message}", MessageDialogStyle.Affirmative, settings);
                }

            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CredentialsChangePasswordDialog
            {
                DataContext = credentialsPasswordViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DisableEncryptionCommand => new RelayCommand(p => DisableEncryptionAction());

        private async Task DisableEncryptionAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.MasterPassword
            };

            var credentialsPasswordViewModel = new CredentialsPasswordViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ProfileManager.DisableEncryption(SelectedProfileFile, instance.Password);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.WrongPassword, Localization.Resources.Strings.WrongPasswordDecryptionFailedMessage, MessageDialogStyle.Affirmative, settings);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.DecryptionError, $"{Localization.Resources.Strings.DecryptionErrorMessage}\n\n{ex.Message}", MessageDialogStyle.Affirmative, settings);
                }

            }, async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CredentialsPasswordDialog
            {
                DataContext = credentialsPasswordViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        #endregion

        #region Methods
        public void SetLocationPathFromDragDrop(string path)
        {
            Location = path;

            OnPropertyChanged(nameof(Location));
        }
        #endregion
    }
}