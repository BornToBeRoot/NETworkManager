using System.Windows.Input;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Helpers;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using NETworkManager.Models.Lookup;
using System.Linq;
using NETworkManager.Utils;

namespace NETworkManager.ViewModels
{
    public class LookupPortLookupViewModel : ViewModelBase
    {
        #region Variables
        private string _portOrService;
        public string PortOrService
        {
            get { return _portOrService; }
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
            get { return _portOrServiceHasError; }
            set
            {
                if (value == _portOrServiceHasError)
                    return;
                _portOrServiceHasError = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _portsOrServicesHistoryView;
        public ICollectionView PortsOrServicesHistoryView
        {
            get { return _portsOrServicesHistoryView; }
        }

        private bool _isLookupRunning;
        public bool IsLookupRunning
        {
            get { return _isLookupRunning; }
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
            get { return _portLookupResult; }
            set
            {
                if (value == _portLookupResult)
                    return;

                _portLookupResult = value;
            }
        }

        private ICollectionView _portLookupResultView;
        public ICollectionView PortLookupResultView
        {
            get { return _portLookupResultView; }
        }

        private PortLookupInfo _selectedPortLookupResult;
        public PortLookupInfo SelectedPortLookupResult
        {
            get { return _selectedPortLookupResult; }
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
            get { return _noPortsFound; }
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
            _portsOrServicesHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_Port_PortsHistory);
            _portLookupResultView = CollectionViewSource.GetDefaultView(PortLookupResult);
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

            List<string> portsByService = new List<string>();

            foreach (string portOrService in PortOrService.Split(';'))
            {
                string portOrService1 = portOrService.Trim();

                if (portOrService1.Contains("-"))
                {
                    string[] portRange = portOrService1.Split('-');

                    if (int.TryParse(portRange[0], out int startPort) && int.TryParse(portRange[1], out int endPort))
                    {
                        if ((startPort > 0) && (startPort < 65536) && (endPort > 0) && (endPort < 65536) && (startPort < endPort))
                        {
                            for (int i = startPort; i < endPort + 1; i++)
                            {
                                foreach (PortLookupInfo info in await PortLookup.LookupAsync(i))
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
                            foreach (PortLookupInfo info in await PortLookup.LookupAsync(port))
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

            foreach (PortLookupInfo info in await PortLookup.LookupByServiceAsync(portsByService))
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
            Clipboard.SetText(SelectedPortLookupResult.Number.ToString());
        }

        public ICommand CopySelectedProtocolCommand
        {
            get { return new RelayCommand(p => CopySelectedProtocolAction()); }
        }

        private void CopySelectedProtocolAction()
        {
            Clipboard.SetText(SelectedPortLookupResult.Protocol.ToString());
        }

        public ICommand CopySelectedServiceCommand
        {
            get { return new RelayCommand(p => CopySelectedServiceAction()); }
        }

        private void CopySelectedServiceAction()
        {
            Clipboard.SetText(SelectedPortLookupResult.Service);
        }

        public ICommand CopySelectedDescriptionCommand
        {
            get { return new RelayCommand(p => CopySelectedDescriptionAction()); }
        }

        public string PortOrService1 { get => _portOrService; set => _portOrService = value; }

        private void CopySelectedDescriptionAction()
        {
            Clipboard.SetText(SelectedPortLookupResult.Description);
        }
        #endregion

        #region  Methods
        private void AddPortOrServiceToHistory(string portOrService)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.Lookup_Port_PortsHistory.ToList(), portOrService, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Lookup_Port_PortsHistory.Clear();
            OnPropertyChanged(nameof(PortOrService)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Lookup_Port_PortsHistory.Add(x));
        }
        #endregion
    }
}