using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Views;
using DnsClient;

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

        private bool _useCache;
        public bool UseCache
        {
            get => _useCache;
            set
            {
                if (value == _useCache)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_UseCache = value;

                _useCache = value;
                OnPropertyChanged();
            }
        }

        public List<QueryClass> QueryClasses { get; set; }

        private QueryClass _queryClass;
        public QueryClass QueryClass
        {
            get => _queryClass;
            set
            {
                if (value == _queryClass)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_QueryClass = value;

                _queryClass = value;
                OnPropertyChanged();
            }
        }

        private bool _showOnlyMostCommonQueryTypes;
        public bool ShowOnlyMostCommonQueryTypes
        {
            get => _showOnlyMostCommonQueryTypes;
            set
            {
                if (value == _showOnlyMostCommonQueryTypes)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes = value;

                _showOnlyMostCommonQueryTypes = value;
                OnPropertyChanged();
            }
        }

        private bool _useTCPOnly;
        public bool UseTCPOnly
        {
            get => _useTCPOnly;
            set
            {
                if (value == _useTCPOnly)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_UseTCPOnly = value;

                _useTCPOnly = value;
                OnPropertyChanged();
            }
        }
               
        private int _retries;
        public int Retries
        {
            get => _retries;
            set
            {
                if (value == _retries)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.DNSLookup_Retries = value;

                _retries = value;
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
            Recursion = SettingsManager.Current.DNSLookup_Recursion;
            UseCache = SettingsManager.Current.DNSLookup_UseCache;
            QueryClasses = System.Enum.GetValues(typeof(QueryClass)).Cast<QueryClass>().OrderBy(x => x.ToString()).ToList();
            QueryClass = QueryClasses.First(x => x == SettingsManager.Current.DNSLookup_QueryClass);
            ShowOnlyMostCommonQueryTypes = SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes;
            UseTCPOnly = SettingsManager.Current.DNSLookup_UseTCPOnly;
            Retries = SettingsManager.Current.DNSLookup_Retries;
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
                Title = Localization.Resources.Strings.AddDNSServer
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
                Title = Localization.Resources.Strings.EditDNSServer
            };

            var dnsServerViewModel = new DNSServerViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
                SettingsManager.Current.DNSLookup_DNSServers.Add(new DNSServerInfo(instance.Name, instance.DNSServer.Replace(" ", "").Split(';').ToList(), instance.Port));
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
                Title = Localization.Resources.Strings.DeleteDNSServer
            };

            var confirmRemoveViewModel = new ConfirmRemoveViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            }, Localization.Resources.Strings.DeleteDNSServerMessage);

            customDialog.Content = new ConfirmRemoveDialog
            {
                DataContext = confirmRemoveViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion
    }
}