using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Dialogs;
using NETworkManager.Views.Dialogs;
using System.Security;
using System;

namespace NETworkManager.ViewModels.Settings
{
    public class CredentialsViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        private bool _credentialsFileExists;
        public bool CredentialsFileExists
        {
            get { return _credentialsFileExists; }
            set
            {
                if (value == _credentialsFileExists)
                    return;

                _credentialsFileExists = value;
                OnPropertyChanged();
            }
        }

        private bool _credentialsLoaded;
        public bool CredentialsLoaded
        {
            get { return _credentialsLoaded; }
            set
            {
                if (value == _credentialsLoaded)
                    return;

                _credentialsLoaded = value;
                OnPropertyChanged();
            }
        }

        ICollectionView _credentials;
        public ICollectionView Credentials
        {
            get { return _credentials; }
        }

        private CredentialInfo _selectedCredential = new CredentialInfo();
        public CredentialInfo SelectedCredential
        {
            get { return _selectedCredential; }
            set
            {
                if (value == _selectedCredential)
                    return;

                _selectedCredential = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public CredentialsViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            CheckCredentialsLoaded();
        }

        private void CheckCredentialsLoaded()
        {
            CredentialsFileExists = File.Exists(CredentialManager.GetCredentialsFilePath());
            CredentialsLoaded = CredentialManager.Loaded;
        }

        private async void Load(SecureString password)
        {
            try
            {
                CredentialManager.Load(password);

                _credentials = CollectionViewSource.GetDefaultView(CredentialManager.Credentials);
                _credentials.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            }
            catch (System.Security.Cryptography.CryptographicException) // If decryption failed
            {
                await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_DecryptionFailed"] as string, Application.Current.Resources["String_DecryptionFailedMessage"] as string, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
            }

            CheckCredentialsLoaded();
        }
        #endregion

        #region Commands & Actions
        public ICommand SetMasterPasswordCommand
        {
            get { return new RelayCommand(p => SetMasterPasswordAction()); }
        }

        private async void SetMasterPasswordAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_SetMasterPassword"] as string
            };

            CredentialsSetMasterPasswordViewModel credentialsSetMasterPasswordViewModel = new CredentialsSetMasterPasswordViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                // Create new collection of credentials and set the password
                Load(instance.Password);

                // Save to file
                CredentialManager.Save();
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CredentialsSetMasterPasswordDialog
            {
                DataContext = credentialsSetMasterPasswordViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand LoadCommand
        {
            get { return new RelayCommand(p => LoadAction()); }
        }

        private async void LoadAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_MasterPassword"] as string
            };

            CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                Load(instance.Password);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CredentialsMasterPasswordDialog
            {
                DataContext = credentialsMasterPasswordViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand ChangeMasterPasswordCommand
        {
            get { return new RelayCommand(p => ChangeMasterPasswordAction()); }
        }

        private async void ChangeMasterPasswordAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_MasterPassword"] as string
            };

            CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                if (CredentialManager.VerifyMasterPasword(instance.Password))
                {
                    CustomDialog customDialog2 = new CustomDialog()
                    {
                        Title = Application.Current.Resources["String_Header_SetMasterPassword"] as string
                    };

                    CredentialsSetMasterPasswordViewModel credentialsSetMasterPasswordViewModel = new CredentialsSetMasterPasswordViewModel(instance2 =>
                    {
                        dialogCoordinator.HideMetroDialogAsync(this, customDialog2);

                        // Set the new master password
                        CredentialManager.SetMasterPassword(instance2.Password);

                        // Save to file
                        CredentialManager.Save();
                    }, instance2 =>
                    {
                        dialogCoordinator.HideMetroDialogAsync(this, customDialog2);
                    });

                    customDialog2.Content = new CredentialsSetMasterPasswordDialog
                    {
                        DataContext = credentialsSetMasterPasswordViewModel
                    };

                    await dialogCoordinator.ShowMetroDialogAsync(this, customDialog2);
                }
                else
                {
                    await dialogCoordinator.ShowMessageAsync(this, "Wrong password!!", "Wrong password message", MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                }
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CredentialsMasterPasswordDialog
            {
                DataContext = credentialsMasterPasswordViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion
    }
}