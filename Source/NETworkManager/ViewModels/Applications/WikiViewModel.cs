using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System.Text.RegularExpressions;

namespace NETworkManager.ViewModels.Applications
{
    public class WikiViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        

        private bool _isLoading = true;

        private string _macOrVendorAddress;
        public string MACAddressOrVendor
        {
            get { return _macOrVendorAddress; }
            set
            {
                if (value == _macOrVendorAddress)
                    return;

                _macOrVendorAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _macAddressOrVendorHasError;
        public bool MACAddressOrVendorHasError
        {
            get { return _macAddressOrVendorHasError; }
            set
            {
                if (value == _macAddressOrVendorHasError)
                    return;

                _macAddressOrVendorHasError = value;
                OnPropertyChanged();
            }
        }

        private List<string> _macAddressOrVendorHistory = new List<string>();
        public List<string> MACAddressOrVendorHistory
        {
            get { return _macAddressOrVendorHistory; }
            set
            {
                if (value == _macAddressOrVendorHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Lookup_MACAddressOrVendorHistory = value;

                _macAddressOrVendorHistory = value;
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

        private bool _noVendorFound;
        public bool NoVendorFound
        {
            get { return _noVendorFound; }
            set
            {
                if (value == _noVendorFound)
                    return;

                _noVendorFound = value;
                OnPropertyChanged();
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

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.Lookup_MACAddressOrVendorHistory != null)
                MACAddressOrVendorHistory = new List<string>(SettingsManager.Current.Lookup_MACAddressOrVendorHistory);

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
            return !MACAddressOrVendorHasError;
        }

        private async void OUILookupAction()
        {
            IsOUILookupRunning = true;

            OUILookupResult.Clear();

            List<string> vendors = new List<string>();

            foreach (string macAddressOrVendor in MACAddressOrVendor.Split(';'))
            {
                string macAddressOrVendor1 = macAddressOrVendor.Trim();

                if (Regex.IsMatch(macAddressOrVendor1, RegexHelper.MACAddressRegex) || Regex.IsMatch(macAddressOrVendor1, RegexHelper.MACAddressFirst3BytesRegex))
                {
                    foreach (OUIInfo info in await OUILookup.LookupAsync(macAddressOrVendor1))
                    {
                        OUILookupResult.Add(info);
                    }
                }
                else
                {
                    vendors.Add(macAddressOrVendor1);
                }
            }

            foreach (OUIInfo info in await OUILookup.LookupByVendorAsync(vendors))
            {
                OUILookupResult.Add(info);
            }

            if (OUILookupResult.Count == 0)
            {
                NoVendorFound = true;
            }
            else
            {
                MACAddressOrVendorHistory = new List<string>(HistoryListHelper.Modify(MACAddressOrVendorHistory, MACAddressOrVendor, SettingsManager.Current.Application_HistoryListEntries));
                NoVendorFound = false;
            }

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

            IsPortLookupRunning = false;
        }
        #endregion
    }
}