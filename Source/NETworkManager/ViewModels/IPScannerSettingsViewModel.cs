using Heijden.DNS;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private bool _useCustomDNSServer;
        public bool UseCustomDNSServer
        {
            get => _useCustomDNSServer;
            set
            {
                if (value == _useCustomDNSServer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_UseCustomDNSServer = value;

                _useCustomDNSServer = value;
                OnPropertyChanged();
            }
        }

        private string _customDNSServer;
        public string CustomDNSServer
        {
            get => _customDNSServer;
            set
            {
                if (value == _customDNSServer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_CustomDNSServer = value.Split(';').ToList();

                _customDNSServer = value;
                OnPropertyChanged();
            }
        }

        private int _dnsPort;
        public int DNSPort
        {
            get => _dnsPort;
            set
            {
                if (value == _dnsPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSPort = value;

                _dnsPort = value;
                OnPropertyChanged();
            }
        }

        private bool _dnsRecursion;
        public bool DNSRecursion
        {
            get => _dnsRecursion;
            set
            {
                if (value == _dnsRecursion)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSRecursion = value;

                _dnsRecursion = value;
                OnPropertyChanged();
            }
        }

        private bool _dnsUseResolverCache;
        public bool DNSUseResolverCache
        {
            get => _dnsUseResolverCache;
            set
            {
                if (value == _dnsUseResolverCache)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSUseResolverCache = value;

                _dnsUseResolverCache = value;
                OnPropertyChanged();
            }
        }

        public List<TransportType> DNSTransportTypes { get; set; }

        private TransportType _dnsTransportType;
        public TransportType DNSTransportType
        {
            get => _dnsTransportType;
            set
            {
                if (value == _dnsTransportType)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSTransportType = value;

                _dnsTransportType = value;
                OnPropertyChanged();
            }
        }

        private int _dnsAttempts;
        public int DNSAttempts
        {
            get => _dnsAttempts;
            set
            {
                if (value == _dnsAttempts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSAttempts = value;

                _dnsAttempts = value;
                OnPropertyChanged();
            }
        }

        private int _dnsTimeout;
        public int DNSTimeout
        {
            get => _dnsTimeout;
            set
            {
                if (value == _dnsTimeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_DNSTimeout = value;

                _dnsTimeout = value;
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

        private CustomCommandInfo _selectedCustomCommand = new CustomCommandInfo();
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

        private bool _showStatistics;
        public bool ShowStatistics
        {
            get => _showStatistics;
            set
            {
                if (value == _showStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.IPScanner_ShowStatistics = value;

                _showStatistics = value;
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
            UseCustomDNSServer = SettingsManager.Current.IPScanner_UseCustomDNSServer;

            if (SettingsManager.Current.IPScanner_CustomDNSServer != null)
                CustomDNSServer = string.Join("; ", SettingsManager.Current.IPScanner_CustomDNSServer);

            DNSPort = SettingsManager.Current.IPScanner_DNSPort;
            DNSRecursion = SettingsManager.Current.IPScanner_DNSRecursion;
            DNSUseResolverCache = SettingsManager.Current.IPScanner_DNSUseResolverCache;
            DNSTransportTypes = System.Enum.GetValues(typeof(TransportType)).Cast<TransportType>().OrderBy(x => x.ToString()).ToList();
            DNSTransportType = DNSTransportTypes.First(x => x == SettingsManager.Current.IPScanner_DNSTransportType);
            DNSAttempts = SettingsManager.Current.IPScanner_DNSAttempts;
            DNSTimeout = SettingsManager.Current.IPScanner_DNSTimeout;
            ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress;
            ShowStatistics = SettingsManager.Current.IPScanner_ShowStatistics;
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
                Title = Resources.Localization.Strings.AddCustomCommand
            };

            var customCommandViewModel = new CustomCommandViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(instance.Name, instance.FilePath, instance.Arguments));
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
                Title = Resources.Localization.Strings.EditCustomCommand
            };

            var customCommandViewModel = new CustomCommandViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
                SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(instance.Name, instance.FilePath, instance.Arguments));
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
                Title = Resources.Localization.Strings.DeleteCustomCommand
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Resources.Localization.Strings.DeleteCustomCommandMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion
    }
}
