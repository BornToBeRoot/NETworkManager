using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System.Text.RegularExpressions;

namespace NETworkManager.ViewModels.Applications
{
    public class WikiOUILookupViewModel : ViewModelBase
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
                    SettingsManager.Current.Wiki_MACAddressOrVendorHistory = value;

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
        #endregion

        #region Constructor, Load settings
        public WikiOUILookupViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.Wiki_MACAddressOrVendorHistory != null)
                MACAddressOrVendorHistory = new List<string>(SettingsManager.Current.Wiki_MACAddressOrVendorHistory);
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
        #endregion
    }
}