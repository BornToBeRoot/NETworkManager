using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;
using NETworkManager.Views;
using NETworkManager.Models.AWS;
using System.ComponentModel;
using System.Windows.Data;

namespace NETworkManager.ViewModels
{
    public class AWSSessionManagerSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;

        private bool _enableSyncInstanceIDsFromAWS;
        public bool EnableSyncInstanceIDsFromAWS
        {
            get => _enableSyncInstanceIDsFromAWS;
            set
            {
                if (value == _enableSyncInstanceIDsFromAWS)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS = value;

                _enableSyncInstanceIDsFromAWS = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView AWSProfiles { get; }

        private AWSProfileInfo _selectedAWSProfile = new AWSProfileInfo();
        public AWSProfileInfo SelectedAWSProfile
        {
            get => _selectedAWSProfile;
            set
            {
                if (value == _selectedAWSProfile)
                    return;

                _selectedAWSProfile = value;
                OnPropertyChanged();
            }
        }

        private bool _syncOnlyRunningInstancesFromAWS;
        public bool SyncOnlyRunningInstancesFromAWS
        {
            get => _syncOnlyRunningInstancesFromAWS;
            set
            {
                if (value == _syncOnlyRunningInstancesFromAWS)
                    return;

                if(!_isLoading)
                    SettingsManager.Current.AWSSessionManager_SyncOnlyRunningInstancesFromAWS = value;

                _syncOnlyRunningInstancesFromAWS = value;
                OnPropertyChanged();
            }
        }

        private string _defaultProfile;
        public string DefaultProfile
        {
            get => _defaultProfile;
            set
            {
                if (value == _defaultProfile)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.AWSSessionManager_DefaultProfile = value;

                _defaultProfile = value;
                OnPropertyChanged();
            }
        }

        private string _defaultRegion;
        public string DefaultRegion
        {
            get => _defaultRegion;
            set
            {
                if (value == _defaultRegion)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.AWSSessionManager_DefaultRegion = value;

                _defaultRegion = value;
                OnPropertyChanged();
            }
        }

        private string _applicationFilePath;
        public string ApplicationFilePath
        {
            get => _applicationFilePath;
            set
            {
                if (value == _applicationFilePath)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.AWSSessionManager_ApplicationFilePath = value;

                IsConfigured = !string.IsNullOrEmpty(value);

                _applicationFilePath = value;
                OnPropertyChanged();
            }
        }

        private bool _isConfigured;
        public bool IsConfigured
        {
            get => _isConfigured;
            set
            {
                if (value == _isConfigured)
                    return;

                _isConfigured = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public AWSSessionManagerSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            AWSProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_AWSProfiles);
            AWSProfiles.SortDescriptions.Add(new SortDescription(nameof(AWSProfileInfo.Profile), ListSortDirection.Ascending));
            AWSProfiles.SortDescriptions.Add(new SortDescription(nameof(AWSProfileInfo.Region), ListSortDirection.Ascending));

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            EnableSyncInstanceIDsFromAWS = SettingsManager.Current.AWSSessionManager_EnableSyncInstanceIDsFromAWS;
            SyncOnlyRunningInstancesFromAWS = SettingsManager.Current.AWSSessionManager_SyncOnlyRunningInstancesFromAWS;
            DefaultProfile = SettingsManager.Current.AWSSessionManager_DefaultProfile;
            DefaultRegion = SettingsManager.Current.AWSSessionManager_DefaultRegion;
            ApplicationFilePath = SettingsManager.Current.AWSSessionManager_ApplicationFilePath;
            IsConfigured = File.Exists(ApplicationFilePath);            
        }
        #endregion

        #region ICommands & Actions
        public ICommand AddAWSProfileCommand => new RelayCommand(p => AddAWSProfileAction());

        private void AddAWSProfileAction()
        {
            AddAWSProfile();
        }

        public ICommand EditAWSProfileCommand => new RelayCommand(p => EditAWSProfileAction());

        private void EditAWSProfileAction()
        {
            EditAWSProfile();
        }

        public ICommand DeleteAWSProfileCommand => new RelayCommand(p => DeleteAWSProfileAction());

        private void DeleteAWSProfileAction()
        {
            DeleteAWSProfile();
        }

        public ICommand BrowseFileCommand => new RelayCommand(p => BrowseFileAction());

        private void BrowseFileAction()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = GlobalStaticConfiguration.ApplicationFileExtensionFilter
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ApplicationFilePath = openFileDialog.FileName;
        }

        public ICommand ConfigureCommand => new RelayCommand(p => ConfigureAction());

        private void ConfigureAction()
        {
            Configure();
        }
        #endregion

        #region Methods        
        public async Task AddAWSProfile()
        {            
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddAWSProfile
            };

            var viewModel = new AWSProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.AWSSessionManager_AWSProfiles.Add(new AWSProfileInfo(instance.IsEnabled, instance.Profile, instance.Region));                
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new AWSProfileDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);            
        }
        
        public async Task EditAWSProfile()
        {            
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.EditAWSProfile
            };

            var viewModel = new AWSProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.AWSSessionManager_AWSProfiles.Remove(SelectedAWSProfile);
                SettingsManager.Current.AWSSessionManager_AWSProfiles.Add(new AWSProfileInfo(instance.IsEnabled, instance.Profile, instance.Region));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, true, SelectedAWSProfile);

            customDialog.Content = new AWSProfileDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);            
        }
                
        public async Task DeleteAWSProfile()
        {            
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.DeleteAWSProfile
            };

            var viewModel = new ConfirmDeleteViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.AWSSessionManager_AWSProfiles.Remove(SelectedAWSProfile);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Localization.Resources.Strings.DeleteAWSProfileMessage);

            customDialog.Content = new ConfirmDeleteDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);            
        }
        
        private async Task Configure()
        {
            try
            {
                Process.Start(SettingsManager.Current.PowerShell_ApplicationFilePath);
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, ex.Message, MessageDialogStyle.Affirmative, settings);
            }
        }

        public void SetFilePathFromDragDrop(string filePath)
        {
            ApplicationFilePath = filePath;

            OnPropertyChanged(nameof(ApplicationFilePath));
        }
        #endregion
    }
}