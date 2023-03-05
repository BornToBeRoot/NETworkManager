using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
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
            Location = ProfileManager.GetProfilesFolderLocation();
        }
        #endregion

        #region ICommands & Actions

        public ICommand OpenLocationCommand => new RelayCommand(p => OpenLocationAction());

        private static void OpenLocationAction()
        {
            Process.Start("explorer.exe", ProfileManager.GetProfilesFolderLocation());
        }
        
        public ICommand AddProfileFileCommand => new RelayCommand(p => AddProfileFileAction());

        private async void AddProfileFileAction()
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

        private async void EditProfileFileAction()
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

        private async void DeleteProfileFileAction()
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

        private async void EnableEncryptionAction()
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

        private async void ChangeMasterPasswordAction()
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

        private async void DisableEncryptionAction()
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
    }
}