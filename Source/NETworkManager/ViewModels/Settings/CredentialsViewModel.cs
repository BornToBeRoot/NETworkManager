using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Dialogs;
using NETworkManager.Views.Dialogs;
using System.Diagnostics;
using NETworkManager.Helpers;
using System;
using System.Windows.Threading;

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

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                Credentials.Refresh();

                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public CredentialsViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            _credentials = CollectionViewSource.GetDefaultView(CredentialManager.Credentials);
            _credentials.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            _credentials.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                CredentialInfo info = o as CredentialInfo;

                string search = Search.Trim();

                // Search by: Name, Username
                return (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Username.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            CheckCredentialsLoaded();
        }

        private void CheckCredentialsLoaded()
        {
            CredentialsFileExists = File.Exists(CredentialManager.GetCredentialsFilePath());
            CredentialsLoaded = CredentialManager.Loaded;
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
                if (CredentialManager.Load(instance.Password))
                    CredentialManager.CredentialsChanged = true; // Save to file when application is closed 

                CheckCredentialsLoaded();
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

        public ICommand DecryptAndLoadCommand
        {
            get { return new RelayCommand(p => DecryptAndLoadAction()); }
        }

        private async void DecryptAndLoadAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_MasterPassword"] as string
            };

            CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                if (!CredentialManager.Load(instance.Password))
                    await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_WrongPassword"] as string, Application.Current.Resources["String_WrongPasswordDecryptionFailed"] as string, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);

                CheckCredentialsLoaded();
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
            CustomDialog customDialogMasterPassword = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_MasterPassword"] as string
            };

            CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
            {
                await dialogCoordinator.HideMetroDialogAsync(this, customDialogMasterPassword);

                if (CredentialManager.VerifyMasterPasword(instance.Password))
                {
                    CustomDialog customDialogSetMasterPassword = new CustomDialog()
                    {
                        Title = Application.Current.Resources["String_Header_SetMasterPassword"] as string
                    };

                    CredentialsSetMasterPasswordViewModel credentialsSetMasterPasswordViewModel = new CredentialsSetMasterPasswordViewModel(instance2 =>
                    {
                        dialogCoordinator.HideMetroDialogAsync(this, customDialogSetMasterPassword);

                        // Set the new master password
                        CredentialManager.SetMasterPassword(instance2.Password);

                        // Save to file
                        CredentialManager.Save();
                    }, instance2 =>
                    {
                        dialogCoordinator.HideMetroDialogAsync(this, customDialogSetMasterPassword);
                    });

                    customDialogSetMasterPassword.Content = new CredentialsSetMasterPasswordDialog
                    {
                        DataContext = credentialsSetMasterPasswordViewModel
                    };

                    await dialogCoordinator.ShowMetroDialogAsync(this, customDialogSetMasterPassword);
                }
                else
                {
                    await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_WrongPassword"] as string, Application.Current.Resources["String_WrongPassword"] as string, MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                }
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialogMasterPassword);
            });

            customDialogMasterPassword.Content = new CredentialsMasterPasswordDialog
            {
                DataContext = credentialsMasterPasswordViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialogMasterPassword);
        }

        public ICommand AddCommand
        {
            get { return new RelayCommand(p => AddAction()); }
        }

        private async void AddAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_AddCredential"] as string
            };

            CredentialViewModel credentialViewModel = new CredentialViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                CredentialInfo credentialInfo = new CredentialInfo
                {
                    ID = instance.ID,
                    Name = instance.Name,
                    Username = instance.Username,
                    Password = instance.Password
                };

                CredentialManager.AddCredential(credentialInfo);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, CredentialManager.GetNextID());

            customDialog.Content = new CredentialDialog
            {
                DataContext = credentialViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand EditCommand
        {
            get { return new RelayCommand(p => EditAction()); }
        }

        private async void EditAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_EditCredential"] as string
            };

            CredentialViewModel credentialViewModel = new CredentialViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                CredentialManager.RemoveCredential(SelectedCredential);

                CredentialInfo credentialInfo = new CredentialInfo
                {
                    ID = instance.ID,
                    Name = instance.Name,
                    Username = instance.Username,
                    Password = instance.Password
                };

                CredentialManager.AddCredential(credentialInfo);

                Debug.WriteLine(SecureStringHelper.ConvertToString(instance.Password));
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, SelectedCredential.ID, SelectedCredential);

            customDialog.Content = new CredentialDialog
            {
                DataContext = credentialViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand DeleteCommand
        {
            get { return new RelayCommand(p => DeleteAction()); }
        }

        private async void DeleteAction()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = Application.Current.Resources["String_Header_DeleteCredential"] as string
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                CredentialManager.RemoveCredential(SelectedCredential);
            }, instance =>
            {
                dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Application.Current.Resources["String_DeleteCredentialMessage"] as string);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion
    }
}