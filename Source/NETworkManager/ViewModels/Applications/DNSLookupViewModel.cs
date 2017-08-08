using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using NETworkManager.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace NETworkManager.ViewModels.Applications
{
    public class DNSLookupViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        CancellationTokenSource cancellationTokenSource;

        private bool _isLoading = true;

        private string _hostnameOrIPAddress;
        public string HostnameOrIPAddress
        {
            get { return _hostnameOrIPAddress; }
            set
            {
                if (value == _hostnameOrIPAddress)
                    return;

                _hostnameOrIPAddress = value;
                OnPropertyChanged();
            }
        }

        private List<string> _hostnameOrIPAddressHistory = new List<string>();
        public List<string> HostnameOrIPAddressHistory
        {
            get { return _hostnameOrIPAddressHistory; }
            set
            {
                if (value == _hostnameOrIPAddressHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Ping_HostnameOrIPAddressHistory = value;

                _hostnameOrIPAddressHistory = value;
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

        private bool _cancelLookup;
        public bool CancelLookup
        {
            get { return _cancelLookup; }
            set
            {
                if (value == _cancelLookup)
                    return;

                _cancelLookup = value;
                OnPropertyChanged();
            }
        }

        private AsyncObservableCollection<PingInfo> _lookupResult = new AsyncObservableCollection<PingInfo>();
        public AsyncObservableCollection<PingInfo> LookupResult
        {
            get { return _lookupResult; }
            set
            {
                if (value == _lookupResult)
                    return;

                _lookupResult = value;
            }
        }
        #endregion

        #region Contructor
        public DNSLookupViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            dialogSettings.CustomResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("NETworkManager;component/Resources/Styles/MetroDialogStyles.xaml", UriKind.RelativeOrAbsolute)
            };

            LoadSettings();

            _isLoading = false;
        }
        #endregion

        #region Load settings
        private void LoadSettings()
        {
            //if (SettingsManager.Current.Ping_HostnameOrIPAddressHistory != null)
            //    HostnameOrIPAddressHistory = new List<string>(SettingsManager.Current.Ping_HostnameOrIPAddressHistory);
        }
        #endregion

        #region ICommands & Actions
        public ICommand LookupCommand
        {
            get { return new RelayCommand(p => LookupAction()); }
        }

        private void LookupAction()
        {
            if (IsLookupRunning)
                StopLookup();
            else
                StartLookup();
        }
        #endregion

        #region Methods      
        private async void StartLookup()
        {
            IsLookupRunning = true;

            // Reset the latest results
            LookupResult.Clear();
        }

        private void StopLookup()
        {
            IsLookupRunning = false;
        }

        public void OnShutdown()
        {
            if (IsLookupRunning)
                LookupAction();
        }
        #endregion
    }
}