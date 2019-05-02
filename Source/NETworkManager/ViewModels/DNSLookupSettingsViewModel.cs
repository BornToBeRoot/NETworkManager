using Heijden.DNS;
using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class DNSLookupSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private readonly IDialogCoordinator _dialogCoordinator;

        public ICollectionView DNSServers { get; }

        private DNSServerInfo _selectedDNSServer = new DNSServerInfo();
        public DNSServerInfo SelectedDNSServer
        {
            get => _selectedDNSServer;
            set
            {
                if (value == _selectedDNSServer)
                    return;

                _selectedDNSServer = value;
                OnPropertyChanged();
            }
        }

        private bool _addDNSSuffix;
        public bool AddDNSSuffix
        {
            get => _addDNSSuffix;
            set
            {
                if (value == _addDNSSuffix)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_AddDNSSuffix = value;

                _addDNSSuffix = value;
                OnPropertyChanged();
            }
        }

        private bool _useCustomDNSSuffix;
        public bool UseCustomDNSSuffix
        {
            get => _useCustomDNSSuffix;
            set
            {
                if (value == _useCustomDNSSuffix)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_UseCustomDNSSuffix = value;

                _useCustomDNSSuffix = value;
                OnPropertyChanged();
            }
        }

        private string _customDNSSuffix;
        public string CustomDNSSuffix
        {
            get => _customDNSSuffix;
            set
            {
                if (value == _customDNSSuffix)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_CustomDNSSuffix = value;

                _customDNSSuffix = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveCNAME;
        public bool ResolveCNAME
        {
            get => _resolveCNAME;
            set
            {
                if (value == _resolveCNAME)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_ResolveCNAME = value;

                _resolveCNAME = value;
                OnPropertyChanged();
            }
        }

        private bool _recursion;
        public bool Recursion
        {
            get => _recursion;
            set
            {
                if (value == _recursion)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Recursion = value;

                _recursion = value;
                OnPropertyChanged();
            }
        }

        private bool _useResolverCache;
        public bool UseResolverCache
        {
            get => _useResolverCache;
            set
            {
                if (value == _useResolverCache)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_UseResolverCache = value;

                _useResolverCache = value;
                OnPropertyChanged();
            }
        }

        public List<QClass> Classes { get; set; }

        private QClass _class;
        public QClass Class
        {
            get => _class;
            set
            {
                if (value == _class)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Class = value;

                _class = value;
                OnPropertyChanged();
            }
        }

        private bool _showMostCommonQueryTypes;
        public bool ShowMostCommonQueryTypes
        {
            get => _showMostCommonQueryTypes;
            set
            {
                if (value == _showMostCommonQueryTypes)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_ShowMostCommonQueryTypes = value;

                _showMostCommonQueryTypes = value;
                OnPropertyChanged();
            }
        }

        public List<TransportType> TransportTypes { get; set; }

        private TransportType _transportType;
        public TransportType TransportType
        {
            get => _transportType;
            set
            {
                if (value == _transportType)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_TransportType = value;

                _transportType = value;
                OnPropertyChanged();
            }
        }

        private int _attempts;
        public int Attempts
        {
            get => _attempts;
            set
            {
                if (value == _attempts)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Attempts = value;

                _attempts = value;
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
                    SettingsManager.Current.DNSLookup_Timeout = value;

                _timeout = value;
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
                    SettingsManager.Current.DNSLookup_ShowStatistics = value;

                _showStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public DNSLookupSettingsViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;

            _dialogCoordinator = instance;
            
            DNSServers = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_DNSServers);
            DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerInfo.Name), ListSortDirection.Ascending));
            DNSServers.Filter = o =>
            {
                if (!(o is DNSServerInfo info))
                    return false;

                return !info.UseWindowsDNSServer;
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix;
            UseCustomDNSSuffix = SettingsManager.Current.DNSLookup_UseCustomDNSSuffix;
            CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix;
            ResolveCNAME = SettingsManager.Current.DNSLookup_ResolveCNAME;
            Recursion = SettingsManager.Current.DNSLookup_Recursion;
            UseResolverCache = SettingsManager.Current.DNSLookup_UseResolverCache;
            Classes = System.Enum.GetValues(typeof(QClass)).Cast<QClass>().OrderBy(x => x.ToString()).ToList();
            Class = Classes.First(x => x == SettingsManager.Current.DNSLookup_Class);
            ShowMostCommonQueryTypes = SettingsManager.Current.DNSLookup_ShowMostCommonQueryTypes;
            TransportTypes = System.Enum.GetValues(typeof(TransportType)).Cast<TransportType>().OrderBy(x => x.ToString()).ToList();
            TransportType = TransportTypes.First(x => x == SettingsManager.Current.DNSLookup_TransportType);
            Attempts = SettingsManager.Current.DNSLookup_Attempts;
            Timeout = SettingsManager.Current.DNSLookup_Timeout;
            ShowStatistics = SettingsManager.Current.DNSLookup_ShowStatistics;
        }
        #endregion

        #region ICommand & Actions
        public ICommand AddDNSServerCommand => new RelayCommand(p => AddDNSServerAction());

        private void AddDNSServerAction()
        {
            AddDNSServer();
        }

        public ICommand EditDNSServerCommand => new RelayCommand(p => EditDNSServerAction());

        private void EditDNSServerAction()
        {
            EditDNSServer();
        }

        public ICommand DeleteDNSServerCommand => new RelayCommand(p => DeleteDNSServerAction());

        private void DeleteDNSServerAction()
        {
            DeleteDNSServer();
        }
        #endregion

        #region Methods

        public async void AddDNSServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddDNSServer
            };

            var dnsServerViewModel = new DNSServerViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Add(new DNSServerInfo(instance.Name, instance.DNSServer.Replace(" ", "").Split(';').ToList(), instance.Port));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new DNSServerDialog
            {
                DataContext = dnsServerViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void EditDNSServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.EditDNSServer
            };

            var dnsServerViewModel = new DNSServerViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
                
                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
                SettingsManager.Current.DNSLookup_DNSServers.Add(new DNSServerInfo(instance.Name, instance.DNSServer.Replace(" ","").Split(';').ToList(), instance.Port));
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, true, SelectedDNSServer);

            customDialog.Content = new DNSServerDialog
            {
                DataContext = dnsServerViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public async void DeleteDNSServer()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.DeleteDNSServer
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Resources.Localization.Strings.DeleteDNSServerMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public void Refresh()
        {
            // Refresh
            DNSServers.Refresh();
        }
        #endregion
    }
}