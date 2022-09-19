using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using NETworkManager.Models.Network;
using NETworkManager.Views;
using NETworkManager.Models.AWS;
using System.ComponentModel;

namespace NETworkManager.ViewModels
{
    public class AWSSessionManagerSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;

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

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
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
            /*
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddDNSServer
            };

            var dnsServerViewModel = new DNSServerViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Add(new DNSServerInfo(instance.Name, instance.DNSServers.Replace(" ", "").Split(';').ToList(), instance.Port));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new DNSServerDialog
            {
                DataContext = dnsServerViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
            */
        }
        
        public async Task EditAWSProfile()
        {
            /*
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.EditDNSServer
            };

            var dnsServerViewModel = new DNSServerViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
                SettingsManager.Current.DNSLookup_DNSServers.Add(new DNSServerInfo(instance.Name, instance.DNSServers.Replace(" ", "").Split(';').ToList(), instance.Port));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, true, SelectedDNSServer);

            customDialog.Content = new DNSServerDialog
            {
                DataContext = dnsServerViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
            */
        }
                
        public async Task DeleteAWSProfile()
        {
            /*
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.DeleteDNSServer
            };

            var confirmDeleteViewModel = new ConfirmDeleteViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Localization.Resources.Strings.DeleteDNSServerMessage);

            customDialog.Content = new ConfirmDeleteDialog
            {
                DataContext = confirmDeleteViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
            */
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