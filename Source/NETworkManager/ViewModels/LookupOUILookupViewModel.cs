using System.Windows.Input;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using NETworkManager.Helpers;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using NETworkManager.Models.Lookup;
using System.Linq;
using NETworkManager.Utils;

namespace NETworkManager.ViewModels
{
    public class LookupOUILookupViewModel : ViewModelBase
    {
        #region Variables
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

        private ICollectionView _macAddressOrVendorHistoryView;
        public ICollectionView MACAddressOrVendorHistoryView
        {
            get { return _macAddressOrVendorHistoryView; }
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

        private ICollectionView _ouiLookupResultView;
        public ICollectionView OUILookupResultView
        {
            get { return _ouiLookupResultView; }
        }

        private OUIInfo _selectedOUILookup;
        public OUIInfo SelectedOUILookup
        {
            get { return _selectedOUILookup; }
            set
            {
                if (value == _selectedOUILookup)
                    return;

                _selectedOUILookup = value;
                OnPropertyChanged();
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
        public LookupOUILookupViewModel()
        {
            // Set collection view
            _macAddressOrVendorHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory);
            _ouiLookupResultView = CollectionViewSource.GetDefaultView(OUILookupResult);
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
            IsLookupRunning = true;

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
                AddMACAddressOrVendorToHistory(MACAddressOrVendor);
                NoVendorFound = false;
            }

            IsLookupRunning = false;
        }

        public ICommand CopySelectedMACAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedMACAddressAction()); }
        }

        private void CopySelectedMACAddressAction()
        {
            Clipboard.SetText(SelectedOUILookup.MACAddress);
        }

        public ICommand CopySelectedVendorCommand
        {
            get { return new RelayCommand(p => CopySelectedVendorAction()); }
        }

        private void CopySelectedVendorAction()
        {
            Clipboard.SetText(SelectedOUILookup.Vendor);
        }
        #endregion

        #region Methods
        private void AddMACAddressOrVendorToHistory(string macAddressOrVendor)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory.ToList(), macAddressOrVendor, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory.Clear();
            OnPropertyChanged(nameof(MACAddressOrVendor)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory.Add(x));
        }
        #endregion
    }
}