using NETworkManager.Models.Network;
using System;
using System.Collections;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Views;
using NETworkManager.Utilities;
using NETworkManager.Models.Settings;
using System.Windows.Threading;
using System.Linq;
using NETworkManager.Models.Export;

namespace NETworkManager.ViewModels
{
    public class ARPTableViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly bool _isLoading;
        private readonly DispatcherTimer _autoRefreshTimer = new DispatcherTimer();
        private bool _isTimerPaused;

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                ARPInfoResultsView.Refresh();

                OnPropertyChanged();
            }
        }

        private ObservableCollection<ARPInfo> _arpInfoResults = new ObservableCollection<ARPInfo>();
        public ObservableCollection<ARPInfo> ARPInfoResults
        {
            get => _arpInfoResults;
            set
            {
                if (value == _arpInfoResults)
                    return;

                _arpInfoResults = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ARPInfoResultsView { get; }

        private ARPInfo _selectedARPInfo;
        public ARPInfo SelectedARPInfo
        {
            get => _selectedARPInfo;
            set
            {
                if (value == _selectedARPInfo)
                    return;

                _selectedARPInfo = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedARPInfos = new ArrayList();
        public IList SelectedARPInfos
        {
            get => _selectedARPInfos;
            set
            {
                if (Equals(value, _selectedARPInfos))
                    return;

                _selectedARPInfos = value;
                OnPropertyChanged();
            }
        }
        
        private bool _autoRefresh;
        public bool AutoRefresh
        {
            get => _autoRefresh;
            set
            {
                if (value == _autoRefresh)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.ARPTable_AutoRefresh = value;

                _autoRefresh = value;

                // Start timer to refresh automatically
                if (!_isLoading)
                {
                    if (value)
                        StartAutoRefreshTimer();
                    else
                        StopAutoRefreshTimer();
                }

                OnPropertyChanged();
            }
        }

        public ICollectionView AutoRefreshTimes { get; }

        private AutoRefreshTimeInfo _selectedAutoRefreshTime;
        public AutoRefreshTimeInfo SelectedAutoRefreshTime
        {
            get => _selectedAutoRefreshTime;
            set
            {
                if (value == _selectedAutoRefreshTime)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.ARPTable_AutoRefreshTime = value;

                _selectedAutoRefreshTime = value;

                if (AutoRefresh)
                    ChangeAutoRefreshTimerInterval(AutoRefreshTime.CalculateTimeSpan(value));

                OnPropertyChanged();
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (value == _isRefreshing)
                    return;

                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get => _displayStatusMessage;
            set
            {
                if (value == _displayStatusMessage)
                    return;

                _displayStatusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (value == _statusMessage)
                    return;

                _statusMessage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public ARPTableViewModel(IDialogCoordinator instance)
        {
            _isLoading = true;
            _dialogCoordinator = instance;

            ARPInfoResultsView = CollectionViewSource.GetDefaultView(ARPInfoResults);
            ARPInfoResultsView.SortDescriptions.Add(new SortDescription(nameof(ARPInfo.IPAddressInt32), ListSortDirection.Ascending));
            ARPInfoResultsView.Filter = o =>
            {
                if (!(o is ARPInfo info))
                    return false;

                if (string.IsNullOrEmpty(Search))
                    return true;

                var filter = Search.Replace(" ", "").Replace("-", "").Replace(":", "");

                // Search by IPAddress and MACAddress
                return info.IPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.MACAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || (info.IsMulticast ? Resources.Localization.Strings.Yes : Resources.Localization.Strings.No).IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1;
            };

            AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.Defaults);
            SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x => (x.Value == SettingsManager.Current.ARPTable_AutoRefreshTime.Value && x.TimeUnit == SettingsManager.Current.ARPTable_AutoRefreshTime.TimeUnit));

            _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

            LoadSettings();

            _isLoading = false;

            Refresh();

            if (AutoRefresh)
                StartAutoRefreshTimer();
        }

        private void LoadSettings()
        {
            AutoRefresh = SettingsManager.Current.ARPTable_AutoRefresh;
        }
        #endregion

        #region ICommands & Actions
        public ICommand RefreshCommand
        {
            get { return new RelayCommand(p => RefreshAction()); }
        }

        private void RefreshAction()
        {
            DisplayStatusMessage = false;

            Refresh();
        }

        public ICommand DeleteTableCommand
        {
            get { return new RelayCommand(p => DeleteTableAction()); }
        }

        private async void DeleteTableAction()
        {
            DisplayStatusMessage = false;

            try
            {
                var arpTable = new ARP();

                arpTable.UserHasCanceled += ArpTable_UserHasCanceled;

                await arpTable.DeleteTableAsync();

                Refresh();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }
        }

        public ICommand DeleteEntryCommand
        {
            get { return new RelayCommand(p => DeleteEntryAction()); }
        }

        private async void DeleteEntryAction()
        {
            DisplayStatusMessage = false;

            try
            {
                var arpTable = new ARP();

                arpTable.UserHasCanceled += ArpTable_UserHasCanceled;

                await arpTable.DeleteEntryAsync(SelectedARPInfo.IPAddress.ToString());

                Refresh();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }
        }

        public ICommand AddEntryCommand
        {
            get { return new RelayCommand(p => AddEntryAction()); }
        }

        private async void AddEntryAction()
        {
            DisplayStatusMessage = false;

            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.AddEntry
            };

            var arpTableAddEntryViewModel = new ArpTableAddEntryViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    var arpTable = new ARP();

                    arpTable.UserHasCanceled += ArpTable_UserHasCanceled;

                    await arpTable.AddEntryAsync(instance.IPAddress, MACAddressHelper.Format(instance.MACAddress, "-"));

                    Refresh();
                }
                catch (Exception ex)
                {
                    StatusMessage = ex.Message;
                    DisplayStatusMessage = true;
                }
            }, instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
            });

            customDialog.Content = new ARPTableAddEntryDialog
            {
                DataContext = arpTableAddEntryViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            CommonMethods.SetClipboard(SelectedARPInfo.IPAddress.ToString());
        }

        public ICommand CopySelectedMACAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedMACAddressAction()); }
        }

        private void CopySelectedMACAddressAction()
        {
            CommonMethods.SetClipboard(MACAddressHelper.GetDefaultFormat(SelectedARPInfo.MACAddress.ToString()));
        }

        public ICommand CopySelectedMulticastCommand
        {
            get { return new RelayCommand(p => CopySelectedMulticastAction()); }
        }

        private void CopySelectedMulticastAction()
        {
            CommonMethods.SetClipboard(SelectedARPInfo.IsMulticast ? Resources.Localization.Strings.Yes : Resources.Localization.Strings.No);
        }

        public ICommand ExportCommand
        {
            get { return new RelayCommand(p => ExportAction()); }
        }

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Resources.Localization.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? ARPInfoResults : new ObservableCollection<ARPInfo>(SelectedARPInfos.Cast<ARPInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Resources.Localization.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Resources.Localization.Strings.Error, Resources.Localization.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.ARPTable_ExportFileType = instance.FileType;
                SettingsManager.Current.ARPTable_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, SettingsManager.Current.ARPTable_ExportFileType, SettingsManager.Current.ARPTable_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private async void Refresh()
        {
            IsRefreshing = true;

            ARPInfoResults.Clear();

            (await ARP.GetTableAsync()).ForEach(x => ARPInfoResults.Add(x));

            Debug.WriteLine("Refresh ARP...");

            IsRefreshing = false;
        }

        private void ChangeAutoRefreshTimerInterval(TimeSpan timeSpan)
        {
            _autoRefreshTimer.Interval = timeSpan;
        }

        private void StartAutoRefreshTimer()
        {
            ChangeAutoRefreshTimerInterval(AutoRefreshTime.CalculateTimeSpan(SelectedAutoRefreshTime));

            _autoRefreshTimer.Start();
        }

        private void StopAutoRefreshTimer()
        {
            _autoRefreshTimer.Stop();
        }

        private void PauseRefresh()
        {
            if (!_autoRefreshTimer.IsEnabled)
                return;

            _autoRefreshTimer.Stop();
            _isTimerPaused = true;
        }

        private void ResumeRefresh()
        {
            if (!_isTimerPaused)
                return;

            _autoRefreshTimer.Start();
            _isTimerPaused = false;
        }
        public void OnViewHide()
        {
            PauseRefresh();
        }

        public void OnViewVisible()
        {
            ResumeRefresh();
        }
        #endregion

        #region Events
        private void ArpTable_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Resources.Localization.Strings.CanceledByUserMessage;
            DisplayStatusMessage = true;
        }

        private void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            Refresh();
        }
        #endregion
    }
}