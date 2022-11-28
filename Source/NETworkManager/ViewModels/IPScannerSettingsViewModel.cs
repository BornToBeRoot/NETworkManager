using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class IPScannerSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private readonly IDialogCoordinator _dialogCoordinator;

        private bool _showScanResultForAllIPAddresses;
        public bool ShowScanResultForAllIPAddresses
        {
            get => _showScanResultForAllIPAddresses;
            set
            {
                if (value == _showScanResultForAllIPAddresses)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ShowScanResultForAllIPAddresses = value;

                _showScanResultForAllIPAddresses = value;
                OnPropertyChanged();
            }
        }

        private int _threads;
        public int Threads
        {
            get => _threads;
            set
            {
                if (value == _threads)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_Threads = value;

                _threads = value;
                OnPropertyChanged();
            }
        }

        private int _icmpTimeout;
        public int ICMPTimeout
        {
            get => _icmpTimeout;
            set
            {
                if (value == _icmpTimeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ICMPTimeout = value;

                _icmpTimeout = value;
                OnPropertyChanged();
            }
        }

        private int _icmpBuffer;
        public int ICMPBuffer
        {
            get => _icmpBuffer;
            set
            {
                if (value == _icmpBuffer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ICMPBuffer = value;

                _icmpBuffer = value;
                OnPropertyChanged();
            }
        }

        private int _icmpAttempts;
        public int ICMPAttempts
        {
            get => _icmpAttempts;
            set
            {
                if (value == _icmpAttempts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ICMPAttempts = value;

                _icmpAttempts = value;
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
                    SettingsManager.Current.IPScanner_ResolveHostname = value;

                _resolveHostname = value;
                OnPropertyChanged();
            }
        }
        
        private bool _dnsShowErrorMessage;
        public bool DNSShowErrorMessage
        {
            get => _dnsShowErrorMessage;
            set
            {
                if (value == _dnsShowErrorMessage)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSShowErrorMessage = value;

                _dnsShowErrorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveMACAddress;
        public bool ResolveMACAddress
        {
            get => _resolveMACAddress;
            set
            {
                if (value == _resolveMACAddress)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ResolveMACAddress = value;

                _resolveMACAddress = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView CustomCommands { get; }

        private CustomCommandInfo _selectedCustomCommand = new();
        public CustomCommandInfo SelectedCustomCommand
        {
            get => _selectedCustomCommand;
            set
            {
                if (value == _selectedCustomCommand)
                    return;

                _selectedCustomCommand = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public IPScannerSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            CustomCommands = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_CustomCommands);
            CustomCommands.SortDescriptions.Add(new SortDescription(nameof(CustomCommandInfo.Name), ListSortDirection.Ascending));

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ShowScanResultForAllIPAddresses = SettingsManager.Current.IPScanner_ShowScanResultForAllIPAddresses;
            Threads = SettingsManager.Current.IPScanner_Threads;
            ICMPTimeout = SettingsManager.Current.IPScanner_ICMPTimeout;
            ICMPBuffer = SettingsManager.Current.IPScanner_ICMPBuffer;
            ICMPAttempts = SettingsManager.Current.IPScanner_ICMPAttempts;
            ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname;            
            DNSShowErrorMessage = SettingsManager.Current.IPScanner_DNSShowErrorMessage;
            ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress;
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddCustomCommandCommand => new RelayCommand(p => AddCustomCommandAction());

        private void AddCustomCommandAction()
        {
            AddCustomCommand();
        }

        public ICommand EditCustomCommandCommand => new RelayCommand(p => EditCustomCommandAction());

        private void EditCustomCommandAction()
        {
            EditCustomCommand();
        }

        public ICommand DeleteCustomCommandCommand => new RelayCommand(p => DeleteCustomCommandAction());

        private void DeleteCustomCommandAction()
        {
            DeleteCustomCommand();
        }

        #endregion

        #region Methods
        public async void AddCustomCommand()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.AddCustomCommand
            };

            var customCommandViewModel = new CustomCommandViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(instance.ID, instance.Name, instance.FilePath, instance.Arguments));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new CustomCommandDialog
            {
                DataContext = customCommandViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void EditCustomCommand()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.EditCustomCommand
            };

            var customCommandViewModel = new CustomCommandViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
                SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(instance.ID, instance.Name, instance.FilePath, instance.Arguments));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, true, SelectedCustomCommand);

            customDialog.Content = new CustomCommandDialog
            {
                DataContext = customCommandViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void DeleteCustomCommand()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.DeleteCustomCommand
            };

            var confirmDeleteViewModel = new ConfirmDeleteViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Localization.Resources.Strings.DeleteCustomCommandMessage);

            customDialog.Content = new ConfirmDeleteDialog
            {
                DataContext = confirmDeleteViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion
    }
}
