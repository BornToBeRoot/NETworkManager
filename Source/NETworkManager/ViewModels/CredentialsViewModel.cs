using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Views;
using System;
using System.Windows.Threading;
using NETworkManager.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class CredentialsViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _dispatcherTimer;
        private const int LockTime = 120; // Seconds remaining until the ui is locked

        private bool _credentialsFileExists;
        public bool CredentialsFileExists
        {
            get => _credentialsFileExists;
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
            get => _credentialsLoaded;
            set
            {
                if (value == _credentialsLoaded)
                    return;

                _credentialsLoaded = value;
                OnPropertyChanged();
            }
        }

        // Indicates that the UI is locked
        private bool _isLocked = true;
        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (value == _isLocked)
                    return;

                _isLocked = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _timeRemaining;
        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                if (value == _timeRemaining)
                    return;

                _timeRemaining = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView Credentials { get; }

        private CredentialInfo _selectedCredential = new CredentialInfo();
        public CredentialInfo SelectedCredential
        {
            get => _selectedCredential;
            set
            {
                if (value == _selectedCredential)
                    return;

                _selectedCredential = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedCredentials = new ArrayList();
        public IList SelectedCredentials
        {
            get => _selectedCredentials;
            set
            {
                if (Equals(value, _selectedCredentials))
                    return;

                _selectedCredentials = value;
                OnPropertyChanged();
            }
        }

        private string _search;
        public string Search
        {
            get => _search;
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
            _dialogCoordinator = instance;

            Credentials = CollectionViewSource.GetDefaultView(CredentialManager.Credentials);
            Credentials.SortDescriptions.Add(new SortDescription(nameof(CredentialInfo.ID), ListSortDirection.Ascending));
            Credentials.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                var search = Search.Trim();

                // Search by: Name, Username
                return o is CredentialInfo info && (info.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || info.Username.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };

            CheckCredentialsLoaded();

            // Set up dispatcher timer
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (TimeRemaining == TimeSpan.Zero)
                TimerLockUiStop();

            TimeRemaining = TimeRemaining.Add(TimeSpan.FromSeconds(-1));
        }

        public void CheckCredentialsLoaded()
        {
            if (CredentialsLoaded)
                return;

            // If file exists, view to decrypt the file is shown
            CredentialsFileExists = File.Exists(CredentialManager.GetCredentialsFilePath());

            // IF credentials are loaded, view to add/edit/remove is shown
            CredentialsLoaded = CredentialManager.Loaded;
        }
        #endregion

        #region Commands & Actions
        public ICommand SetMasterPasswordCommand
        {
            get { return new RelayCommand(p => SetMasterPasswordAction()); }
        }

        private void SetMasterPasswordAction()
        {
            SetMasterPassword();
        }

        public ICommand DecryptAndLoadCommand
        {
            get { return new RelayCommand(p => DecryptAndLoadAction()); }
        }

        private void DecryptAndLoadAction()
        {
            DecryptAndLoad();
        }

        public ICommand ChangeMasterPasswordCommand
        {
            get { return new RelayCommand(p => ChangeMasterPasswordAction()); }
        }

        private void ChangeMasterPasswordAction()
        {
            ChangeMasterPassword();
        }

        public ICommand AddCommand
        {
            get { return new RelayCommand(p => AddAction()); }
        }

        private void AddAction()
        {
            Add();
        }

        public ICommand EditCommand
        {
            get { return new RelayCommand(p => EditAction()); }
        }

        private void EditAction()
        {
            Edit();
        }

        public ICommand DeleteCommand
        {
            get { return new RelayCommand(p => DeleteAction()); }
        }

        private void DeleteAction()
        {
            Delete();
        }

        public ICommand LockUnlockCommand
        {
            get { return new RelayCommand(p => LockUnlockAction()); }
        }

        private void LockUnlockAction()
        {
            LockUnlock();
        }
        #endregion

        #region Methods
        public async void SetMasterPassword()
        {
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_SetMasterPassword")
            };

            var credentialsSetMasterPasswordViewModel = new CredentialsSetMasterPasswordViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                // Create new collection of credentials and set the password
                if (CredentialManager.Load(instance.Password))
                    CredentialManager.CredentialsChanged = true; // Save to file when application is closed 

                CheckCredentialsLoaded();

                TimerLockUiStart();
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CredentialsSetMasterPasswordDialog
            {
                DataContext = credentialsSetMasterPasswordViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void DecryptAndLoad()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_MasterPassword")
            };

            CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                if (!CredentialManager.Load(instance.Password))
                    await _dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_WrongPassword"), LocalizationManager.GetStringByKey("String_WrongPasswordDecryptionFailed"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);

                CheckCredentialsLoaded();

                TimerLockUiStart();
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CredentialsMasterPasswordDialog
            {
                DataContext = credentialsMasterPasswordViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void ChangeMasterPassword()
        {
            var customDialogSetMasterPassword = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_SetMasterPassword")
            };

            var credentialsSetMasterPasswordViewModel = new CredentialsSetMasterPasswordViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialogSetMasterPassword);

                // Set the new master password
                CredentialManager.SetMasterPassword(instance.Password);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialogSetMasterPassword);
            });

            customDialogSetMasterPassword.Content = new CredentialsSetMasterPasswordDialog
            {
                DataContext = credentialsSetMasterPasswordViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialogSetMasterPassword);
        }

        public async void Add()
        {
            var customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_AddCredential")
            };

            var credentialViewModel = new CredentialViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                CredentialInfo credentialInfo = new CredentialInfo
                {
                    ID = instance.Id,
                    Name = instance.Name,
                    Username = instance.Username,
                    Password = instance.Password
                };

                CredentialManager.AddCredential(credentialInfo);

                TimerLockUiStart(); // Reset timer
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, CredentialManager.GetNextID());

            customDialog.Content = new CredentialDialog
            {
                DataContext = credentialViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void Edit()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_EditCredential")
            };

            CredentialViewModel credentialViewModel = new CredentialViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                CredentialManager.RemoveCredential(SelectedCredential);

                CredentialInfo credentialInfo = new CredentialInfo
                {
                    ID = instance.Id,
                    Name = instance.Name,
                    Username = instance.Username,
                    Password = instance.Password
                };

                CredentialManager.AddCredential(credentialInfo);

                TimerLockUiStart(); // Reset timer
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, SelectedCredential.ID, SelectedCredential);

            customDialog.Content = new CredentialDialog
            {
                DataContext = credentialViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void Delete()
        {
            CustomDialog customDialog = new CustomDialog()
            {
                Title = LocalizationManager.GetStringByKey("String_Header_DeleteCredential")
            };

            ConfirmRemoveViewModel confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                List<CredentialInfo> list = new List<CredentialInfo>(SelectedCredentials.Cast<CredentialInfo>());

                foreach (CredentialInfo credential in list)
                    CredentialManager.RemoveCredential(credential);

                TimerLockUiStart(); // Reset timer
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, LocalizationManager.GetStringByKey("String_DeleteCredentialMessage"));

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void LockUnlock()
        {
            if (IsLocked)
            {
                CustomDialog customDialogMasterPassword = new CustomDialog()
                {
                    Title = LocalizationManager.GetStringByKey("String_Header_MasterPassword")
                };

                CredentialsMasterPasswordViewModel credentialsMasterPasswordViewModel = new CredentialsMasterPasswordViewModel(async instance =>
                {
                    await _dialogCoordinator.HideMetroDialogAsync(this, customDialogMasterPassword);

                    if (CredentialManager.VerifyMasterPasword(instance.Password))
                        TimerLockUiStart();
                    else
                        await _dialogCoordinator.ShowMessageAsync(this, LocalizationManager.GetStringByKey("String_Header_WrongPassword"), LocalizationManager.GetStringByKey("String_WrongPassword"), MessageDialogStyle.Affirmative, AppearanceManager.MetroDialog);
                }, instance =>
                {
                    _dialogCoordinator.HideMetroDialogAsync(this, customDialogMasterPassword);
                });

                customDialogMasterPassword.Content = new CredentialsMasterPasswordDialog
                {
                    DataContext = credentialsMasterPasswordViewModel
                };

                await _dialogCoordinator.ShowMetroDialogAsync(this, customDialogMasterPassword);
            }
            else
            {
                TimerLockUiStop();
            }
        }

        private void TimerLockUiStart()
        {
            IsLocked = false;

            TimeRemaining = TimeSpan.FromSeconds(LockTime);

            _dispatcherTimer.Start();
        }

        private void TimerLockUiStop()
        {
            _dispatcherTimer.Stop();

            IsLocked = true;
        }
        #endregion
    }
}