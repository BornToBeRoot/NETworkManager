using System.Windows.Input;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Helpers;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using NETworkManager.Models.Lookup;

namespace NETworkManager.ViewModels.Applications
{
    public class LookupPortLookupViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private string _portsOrService;
        public string PortsOrService
        {
            get { return _portsOrService; }
            set
            {
                if (value == _portsOrService)
                    return;

                _portsOrService = value;
                OnPropertyChanged();
            }
        }

        private bool _portsOrServiceHasError;
        public bool PortsOrServiceHasError
        {
            get { return _portsOrServiceHasError; }
            set
            {
                if (value == _portsOrServiceHasError)
                    return;
                _portsOrServiceHasError = value;
                OnPropertyChanged();
            }
        }

        private List<string> _portsHistory = new List<string>();
        public List<string> PortsHistory
        {
            get { return _portsHistory; }
            set
            {
                if (value == _portsHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Lookup_PortsHistory = value;

                _portsHistory = value;
                OnPropertyChanged();
            }
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
            _portLookupResultView = CollectionViewSource.GetDefaultView(PortLookupResult);

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.Lookup_PortsHistory != null)
                PortsHistory = new List<string>(SettingsManager.Current.Lookup_PortsHistory);
        }
        #endregion

        #region ICommands & Actions
        public ICommand PortLookupCommand
        {
            get { return new RelayCommand(p => PortLookupAction(), PortLookup_CanExecute); }
        }

        private bool PortLookup_CanExecute(object parameter)
        {
            return !PortsOrServiceHasError;
        }

        private async void PortLookupAction()
        {
            IsLookupRunning = true;

            PortLookupResult.Clear();

            List<string> portsByService = new List<string>();

            foreach (string portOrService in PortsOrService.Split(';'))
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
                PortsHistory = new List<string>(HistoryListHelper.Modify(PortsHistory, PortsOrService, SettingsManager.Current.Application_HistoryListEntries));
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

        private void CopySelectedDescriptionAction()
        {
            Clipboard.SetText(SelectedPortLookupResult.Description);
        }
        #endregion
    }
}