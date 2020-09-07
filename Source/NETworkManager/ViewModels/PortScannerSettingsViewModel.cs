using NETworkManager.Settings;
using NETworkManager.Models.Network;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class PortScannerSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private readonly IDialogCoordinator _dialogCoordinator;

        public ICollectionView PortProfiles { get; }

        private PortProfileInfo _selectedPortProfile = new PortProfileInfo();
        public PortProfileInfo SelectedPortProfile
        {
            get => _selectedPortProfile;
            set
            {
                if (value == _selectedPortProfile)
                    return;

                _selectedPortProfile = value;
                OnPropertyChanged();
            }

        }

        private int _hostThreads;
        public int HostThreads
        {
            get => _hostThreads;
            set
            {
                if (value == _hostThreads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_HostThreads = value;

                _hostThreads = value;
                OnPropertyChanged();
            }
        }

        private int _portThreads;
        public int PortThreads
        {
            get => _portThreads;
            set
            {
                if (value == _portThreads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_PortThreads = value;

                _portThreads = value;
                OnPropertyChanged();
            }
        }

        private bool _showClosed;
        public bool ShowClosed
        {
            get => _showClosed;
            set
            {
                if (value == _showClosed)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ShowClosed = value;

                _showClosed = value;
                OnPropertyChanged();
            }
        }

        private int _timeout;
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostname;
        public bool ResolveHostname
        {
            get => _resolveHostname;
            set
            {
                if (value == _resolveHostname)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.PortScanner_ResolveHostname = value;

                _resolveHostname = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public PortScannerSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            PortProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortProfiles);
            PortProfiles.SortDescriptions.Add(new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            HostThreads = SettingsManager.Current.PortScanner_HostThreads;
            PortThreads = SettingsManager.Current.PortScanner_PortThreads;
            ShowClosed = SettingsManager.Current.PortScanner_ShowClosed;
            Timeout = SettingsManager.Current.PortScanner_Timeout;
            ResolveHostname = SettingsManager.Current.PortScanner_ResolveHostname;
        }
        #endregion
        
        #region ICommand & Actions
        public ICommand AddPortProfileCommand => new RelayCommand(p => AddPortProfileAction());

        private void AddPortProfileAction()
        {
            AddPortProfile();
        }

        public ICommand EditPortProfileCommand => new RelayCommand(p => EditPortProfileAction());

        private void EditPortProfileAction()
        {
            EditPortProfile();
        }

        public ICommand DeletePortProfileCommand => new RelayCommand(p => DeletePortProfileAction());

        private void DeletePortProfileAction()
        {
            DeletePortProfile();
        }
        #endregion

        
        #region Methods
        

        public async void AddPortProfile()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddPortProfile
            };

            var viewModel = new PortProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name, instance.Ports));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new PortProfileDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void EditPortProfile()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.EditPortProfile
            };

            var viewModel = new PortProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
                SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name, instance.Ports));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, true, SelectedPortProfile);

            customDialog.Content = new PortProfileDialog
            {
                DataContext = viewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void DeletePortProfile()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.DeletePortProfile
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Localization.Resources.Strings.DeletePortProfileMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion                
    }
}