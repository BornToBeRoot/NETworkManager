using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Helpers;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Dialogs;
using NETworkManager.Views.Dialogs;

namespace NETworkManager.ViewModels.Settings
{
    public class CredentialsViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;

        private bool _fileExists;
        public bool FileExists
        {
            get { return _fileExists; }
            set
            {
                if (value == _fileExists)
                    return;

                _fileExists = value;
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

            FileExists = File.Exists(CredentialManager.GetCredentialsFilePath());
        }

        private void LoadCredentials()
        {
            CredentialManager.Load(SecureStringHelper.ConvertToSecureString("TEST"));

            _credentials = CollectionViewSource.GetDefaultView(CredentialManager.Credentials);
            _credentials.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
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

                // Set password here... instance.Pasword...

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