using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System.Threading.Tasks;
using System.IO;

namespace NETworkManager.ViewModels.Applications
{
    public class OUILookupViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        MetroDialogSettings dialogSettings = new MetroDialogSettings();

//        CancellationTokenSource cancellationTokenSource;

        private bool _isLoading = true;

        private string _macAddressOrFirst3Bytes;
        public string MACAddressOrFirst3Bytes
        {
            get { return _macAddressOrFirst3Bytes; }
            set
            {
                if (value == _macAddressOrFirst3Bytes)
                    return;

                _macAddressOrFirst3Bytes = value;
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
                {
                    SettingsManager.Current.OUILookup_MACAddressHistory = value;
                }

                _macAddressHistory = value;
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

        private ObservableCollection<OUIInfo> _lookupResult = new ObservableCollection<OUIInfo>();
        public ObservableCollection<OUIInfo> LookupResult
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

        #region Constructor, Load settings
        public OUILookupViewModel(IDialogCoordinator instance)
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
            if (SettingsManager.Current.Traceroute_HostnameOrIPAddressHistory != null)
                MACAddressHistory = new List<string>(SettingsManager.Current.OUILookup_MACAddressHistory);
        }
        #endregion

        #region Settings

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


        private void StopLookup()
        {
            CancelLookup = true;
            //cancellationTokenSource.Cancel();
        }

        private async void StartLookup()
        {
            IsLookupRunning = true;
            LookupResult.Clear();

            foreach(string macAddress in MACAddressOrFirst3Bytes.Replace(" ","").Split(';'))
            {
                LookupResult.Add(await OUILookup.LookupAsync(macAddress));
            }            
            
            MACAddressHistory = new List<string>(HistoryListHelper.Modify(MACAddressHistory, MACAddressOrFirst3Bytes, 5));                       

            IsLookupRunning = false;
        }
        #endregion

        #region OnShutdown
        public void OnShutdown()
        {
            if (IsLookupRunning)
                StopLookup();
        }
        #endregion
    }
}