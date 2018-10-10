using System.Windows.Input;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Models.Lookup;
using System.Linq;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class LookupPortLookupViewModel : ViewModelBase
    {
        #region Variables
        private string _portOrService;
        public string PortOrService
        {
            get => _portOrService;
            set
            {
                if (value == _portOrService)
                    return;

                _portOrService = value;
                OnPropertyChanged();
            }
        }

        private bool _portOrServiceHasError;
        public bool PortOrServiceHasError
        {
            get => _portOrServiceHasError;
            set
            {
                if (value == _portOrServiceHasError)
                    return;
                _portOrServiceHasError = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView PortsOrServicesHistoryView { get; }

        private bool _isLookupRunning;
        public bool IsLookupRunning
        {
            get => _isLookupRunning;
            set
            {
                if (value == _isLookupRunning)
                    return;

                _isLookupRunning = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PortLookupInfo> _portLookupResult = new ObservableCollection<PortLookupInfo>();
        public ObservableCollection<PortLookupInfo> PortLookupResult
        {
            get => _portLookupResult;
            set
            {
                if (value != null && value == _portLookupResult)
                    return;

                _portLookupResult = value;
            }
        }

        public ICollectionView PortLookupResultView { get; }

        private PortLookupInfo _selectedPortLookupResult;
        public PortLookupInfo SelectedPortLookupResult
        {
            get => _selectedPortLookupResult;
            set
            {
                if (value == _selectedPortLookupResult)
                    return;

                _selectedPortLookupResult = value;
                OnPropertyChanged();
            }
        }

        private bool _noPortsFound;
        public bool NoPortsFound
        {
            get => _noPortsFound;
            set
            {
                if (value == _noPortsFound)
                    return;

                _noPortsFound = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, Load settings
        public LookupPortLookupViewModel()
        {
            PortsOrServicesHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_Port_PortsHistory);
            PortLookupResultView = CollectionViewSource.GetDefaultView(PortLookupResult);
        }
        #endregion

        #region ICommands & Actions
        public ICommand PortLookupCommand
        {
            get { return new RelayCommand(p => PortLookupAction(), PortLookup_CanExecute); }
        }

        private bool PortLookup_CanExecute(object parameter)
        {
            return !PortOrServiceHasError;
        }

        private async void PortLookupAction()
        {
            IsLookupRunning = true;

            PortLookupResult.Clear();

            var portsByService = new List<string>();

            foreach (var portOrService in PortOrService.Split(';'))
            {
                var portOrService1 = portOrService.Trim();

                if (portOrService1.Contains("-"))
                {
                    var portRange = portOrService1.Split('-');

                    if (int.TryParse(portRange[0], out var startPort) && int.TryParse(portRange[1], out var endPort))
                    {
                        if ((startPort > 0) && (startPort < 65536) && (endPort > 0) && (endPort < 65536) && (startPort < endPort))
                        {
                            for (var i = startPort; i < endPort + 1; i++)
                            {
                                foreach (var info in await PortLookup.LookupAsync(i))
                                {
                                    PortLookupResult.Add(info);
                                }
                            }
                        }
                        else
                        {
                            portsByService.Add(portOrService1);
                        }

                    }
                    else
                    {
                        portsByService.Add(portOrService1);
                    }
                }
                else
                {
                    if (int.TryParse(portOrService1, out int port))
                    {
                        if (port > 0 && port < 65536)
                        {
                            foreach (var info in await PortLookup.LookupAsync(port))
                            {
                                PortLookupResult.Add(info);
                            }
                        }
                        else
                        {
                            portsByService.Add(portOrService1);
                        }
                    }
                    else
                    {
                        portsByService.Add(portOrService1);
                    }
                }
            }

            foreach (var info in await PortLookup.LookupByServiceAsync(portsByService))
            {
                PortLookupResult.Add(info);
            }

            if (PortLookupResult.Count == 0)
            {
                NoPortsFound = true;
            }
            else
            {
                AddPortOrServiceToHistory(PortOrService);
                NoPortsFound = false;
            }

            IsLookupRunning = false;
        }

        public ICommand CopySelectedPortCommand
        {
            get { return new RelayCommand(p => CopySelectedPortAction()); }
        }

        private void CopySelectedPortAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Number.ToString());
        }

        public ICommand CopySelectedProtocolCommand
        {
            get { return new RelayCommand(p => CopySelectedProtocolAction()); }
        }

        private void CopySelectedProtocolAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Protocol.ToString());
        }

        public ICommand CopySelectedServiceCommand
        {
            get { return new RelayCommand(p => CopySelectedServiceAction()); }
        }

        private void CopySelectedServiceAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Service);
        }

        public ICommand CopySelectedDescriptionCommand
        {
            get { return new RelayCommand(p => CopySelectedDescriptionAction()); }
        }

        private void CopySelectedDescriptionAction()
        {
            CommonMethods.SetClipboard(SelectedPortLookupResult.Description);
        }
        #endregion

        #region  Methods
        private void AddPortOrServiceToHistory(string portOrService)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.Lookup_Port_PortsHistory.ToList(), portOrService, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Lookup_Port_PortsHistory.Clear();
            OnPropertyChanged(nameof(PortOrService)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Lookup_Port_PortsHistory.Add(x));
        }
        #endregion
    }
}