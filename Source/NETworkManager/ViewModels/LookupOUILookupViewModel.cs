using System;
using System.Collections;
using System.Windows.Input;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Models.Lookup;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class LookupOUILookupViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        
        private string _macOrVendorAddress;
        public string MACAddressOrVendor
        {
            get => _macOrVendorAddress;
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
            get => _macAddressOrVendorHasError;
            set
            {
                if (value == _macAddressOrVendorHasError)
                    return;

                _macAddressOrVendorHasError = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView MACAddressOrVendorHistoryView { get; }

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

        private ObservableCollection<OUIInfo> _ouiLookupResults = new ObservableCollection<OUIInfo>();
        public ObservableCollection<OUIInfo> OUILookupResults
        {
            get => _ouiLookupResults;
            set
            {
                if (value != null && value == _ouiLookupResults)
                    return;

                _ouiLookupResults = value;
            }
        }

        public ICollectionView OUILookupResultsView { get; }

        private OUIInfo _selectedOUILookupResult;
        public OUIInfo SelectedOUILookupResult
        {
            get => _selectedOUILookupResult;
            set
            {
                if (value == _selectedOUILookupResult)
                    return;

                _selectedOUILookupResult = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedOUILookupResults = new ArrayList();
        public IList SelectedOUILookupResults
        {
            get => _selectedOUILookupResults;
            set
            {
                if (Equals(value, _selectedOUILookupResults))
                    return;

                _selectedOUILookupResults = value;
                OnPropertyChanged();
            }
        }

        private bool _noVendorFound;
        public bool NoVendorFound
        {
            get => _noVendorFound;
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
        public LookupOUILookupViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            // Set collection view
            MACAddressOrVendorHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory);
            OUILookupResultsView = CollectionViewSource.GetDefaultView(OUILookupResults);
        }
        #endregion

        #region ICommands & Actions
        public ICommand OUILookupCommand => new RelayCommand(p => OUILookupAction(), OUILookup_CanExecute);

        private bool OUILookup_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen && !MACAddressOrVendorHasError;

        private async void OUILookupAction()
        {
            IsLookupRunning = true;

            OUILookupResults.Clear();

            var vendors = new List<string>();

            foreach (var macAddressOrVendor in MACAddressOrVendor.Split(';'))
            {
                var macAddressOrVendor1 = macAddressOrVendor.Trim();

                if (Regex.IsMatch(macAddressOrVendor1, RegexHelper.MACAddressRegex) || Regex.IsMatch(macAddressOrVendor1, RegexHelper.MACAddressFirst3BytesRegex))
                {
                    foreach (var info in await OUILookup.LookupAsync(macAddressOrVendor1))
                    {
                        OUILookupResults.Add(info);
                    }
                }
                else
                {
                    vendors.Add(macAddressOrVendor1);
                }
            }

            foreach (var info in await OUILookup.LookupByVendorAsync(vendors))
            {
                OUILookupResults.Add(info);
            }

            if (OUILookupResults.Count == 0)
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

        public ICommand CopySelectedMACAddressCommand => new RelayCommand(p => CopySelectedMACAddressAction());

        private void CopySelectedMACAddressAction()
        {
            ClipboardHelper.SetClipboard(SelectedOUILookupResult.MACAddress);
        }

        public ICommand CopySelectedVendorCommand => new RelayCommand(p => CopySelectedVendorAction());

        private void CopySelectedVendorAction()
        {
            ClipboardHelper.SetClipboard(SelectedOUILookupResult.Vendor);
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? OUILookupResults : new ObservableCollection<OUIInfo>(SelectedOUILookupResults.Cast<OUIInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.Lookup_OUI_ExportFileType = instance.FileType;
                SettingsManager.Current.Lookup_OUI_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, SettingsManager.Current.Lookup_OUI_ExportFileType, SettingsManager.Current.Lookup_OUI_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private void AddMACAddressOrVendorToHistory(string macAddressOrVendor)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory.ToList(), macAddressOrVendor, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory.Clear();
            OnPropertyChanged(nameof(MACAddressOrVendor)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Lookup_OUI_MACAddressOrVendorHistory.Add(x));
        }
        #endregion
    }
}