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

        private string _ports;
        public string Ports
        {
            get { return _ports; }
            set
            {
                if (value == _ports)
                    return;

                _ports = value;
                OnPropertyChanged();
            }
        }

        private bool _portsHasError;
        public bool PortsHasError
        {
            get { return _portsHasError; }
            set
            {
                if (value == _portsHasError)
                    return;
                _portsHasError = value;
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

        #region Settings

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
            get { return new RelayCommand(p => PortLookupAction(),PortLookup_CanExecute); }
        }

        private bool PortLookup_CanExecute(object parameter)
        {
            return !PortsHasError;
        }

        private async void PortLookupAction()
        {
            IsPortLookupRunning = true;

            PortLookupResult.Clear();

            int[] ports = await PortRangeHelper.ConvertPortRangeToIntArrayAsync(Ports);

            foreach (PortLookupInfo info in await PortLookup.LookupAsync(ports))
            {
                PortLookupResult.Add(info);
            }

            PortsHistory = new List<string>(HistoryListHelper.Modify(PortsHistory, Ports, SettingsManager.Current.Application_HistoryListEntries));

            IsPortLookupRunning = false;
        }
        #endregion
    }
}