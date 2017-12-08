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

        private void Load(SecureString password)
        {
            CredentialManager.Load(password);

            _credentials = CollectionViewSource.GetDefaultView(CredentialManager.Credentials);
            _credentials.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));

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

                Load(instance.Password);
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
        #endregion
    }
}