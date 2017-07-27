using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using NETworkManager.Helpers;

namespace NETworkManager.ViewModels.Applications
{
    public class WikiViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        private bool _isLoading = true;

        private string _macAddress;
        public string MACAddress
        {
            get { return _macAddress; }
            set
            {
                if (value == _macAddress)
                    return;

                _macAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _macAddressHasError;
        public bool MACAddressHasError
        {
            get { return _macAddressHasError; }
            set
            {
                if (value == _macAddressHasError)
                    return;

                _macAddressHasError = value;
                OnPropertyChanged();
            }
        }

        private List<string> _macAddressHistory = new List<string>();
        public List<string> MACAddressHistory
        {
            get { return _macAddressHistory; }
            set
            {
                if (value == _macAddressHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Lookup_MACAddressHistory = value;

                _macAddressHistory = value;
                OnPropertyChanged();
            }
        }

        private bool _isOUILookupRunning;
        public bool IsOUILookupRunning
        {
            get { return _isOUILookupRunning; }
            set
            {
                if (value == _isOUILookupRunning)
                    return;

                _isOUILookupRunning = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<OUIInfo> _ouiLookupResult = new ObservableCollection<OUIInfo>();
        public ObservableCollection<OUIInfo> OUILookupResult
        {
            get { return _ouiLookupResult; }
            set
            {
                if (value == _ouiLookupResult)
                    return;

                _ouiLookupResult = value;
            }
        }

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

        private bool _isPortLookupRunning;
        public bool IsPortLookupRunning
        {
            get { return _isPortLookupRunning; }
            set
            {
                if (value == _isPortLookupRunning)
                    return;

                _isPortLookupRunning = value;
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
        public WikiViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.Lookup_MACAddressHistory != null)
                MACAddressHistory = new List<string>(SettingsManager.Current.Lookup_MACAddressHistory);

            if (SettingsManager.Current.Lookup_PortsHistory != null)
                PortsHistory = new List<string>(SettingsManager.Current.Lookup_PortsHistory);
        }
        #endregion

        #region ICommands & Actions
        public ICommand OUILookupCommand
        {
            get { return new RelayCommand(p => OUILookupAction(), OUILookup_CanExecute); }
        }

        private bool OUILookup_CanExecute(object parameter)
        {
            return !MACAddressHasError;
        }

        private async void OUILookupAction()
        {
            IsOUILookupRunning = true;

            OUILookupResult.Clear();

            foreach (string macAddress in MACAddress.Replace(" ", "").Split(';'))
            {
                foreach (OUIInfo info in await OUILookup.LookupAsync(macAddress))
                {
                    OUILookupResult.Add(info);
                }
            }

            MACAddressHistory = new List<string>(HistoryListHelper.Modify(MACAddressHistory, MACAddress, SettingsManager.Current.Application_HistoryListEntries));

            IsOUILookupRunning = false;
        }

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
            IsPortLookupRunning = true;

            PortLookupResult.Clear();

            List<int> ports = new List<int>();
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
                            for (int i = startPort; i < endPort +1; i++)
                            {
                                ports.Add(i);
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
                            ports.Add(port);
                        else
                            portsByService.Add(portOrService1);
                    }
                    else
                    {
                        portsByService.Add(portOrService1);
                    }
                }
            }

            foreach (PortLookupInfo info in await PortLookup.LookupAsync(ports))
            {
                PortLookupResult.Add(info);
            }

            foreach(PortLookupInfo info in await PortLookup.LookupByServiceAsync(portsByService))
            {
                PortLookupResult.Add(info);
            }
                       
            PortsHistory = new List<string>(HistoryListHelper.Modify(PortsHistory, PortsOrService, SettingsManager.Current.Application_HistoryListEntries));

            IsPortLookupRunning = false;

            NoPortsFound = PortLookupResult.Count == 0;
        }
        #endregion
    }
}