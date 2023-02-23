using NETworkManager.Models.Network;
using NETworkManager.Settings;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using NETworkManager.Models.Export;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class SNTPLookupViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        public readonly int TabId;

        private readonly bool _isLoading;

        public ICollectionView SNTPServers { get; }

        private ServerInfoProfile _sntpServer = new();
        public ServerInfoProfile SNTPServer
        {
            get => _sntpServer;
            set
            {
                if (value == _sntpServer)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SNTPLookup_SelectedSNTPServer = value;

                _sntpServer = value;
                OnPropertyChanged();
            }
        }

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

        private ObservableCollection<SNTPLookupResultInfo> _lookupResults = new();
        public ObservableCollection<SNTPLookupResultInfo> LookupResults
        {
            get => _lookupResults;
            set
            {
                if (Equals(value, _lookupResults))
                    return;

                _lookupResults = value;
            }
        }

        public ICollectionView LookupResultsView { get; }

        private SNTPLookupResultInfo _selectedLookupResult;
        public SNTPLookupResultInfo SelectedLookupResult
        {
            get => _selectedLookupResult;
            set
            {
                if (value == _selectedLookupResult)
                    return;

                _selectedLookupResult = value;
                OnPropertyChanged();
            }
        }

        private IList _selectedLookupResults = new ArrayList();
        public IList SelectedLookupResults
        {
            get => _selectedLookupResults;
            set
            {
                if (Equals(value, _selectedLookupResults))
                    return;

                _selectedLookupResults = value;
                OnPropertyChanged();
            }
        }

        private bool _isStatusMessageDisplayed;
        public bool IsStatusMessageDisplayed
        {
            get => _isStatusMessageDisplayed;
            set
            {
                if (value == _isStatusMessageDisplayed)
                    return;

                _isStatusMessageDisplayed = value;
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
        public SNTPLookupViewModel(IDialogCoordinator instance, int tabId)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            TabId = tabId;

            SNTPServers = new CollectionViewSource { Source = SettingsManager.Current.SNTPLookup_SNTPServers }.View;
            SNTPServers.SortDescriptions.Add(new SortDescription(nameof(ServerInfoProfile.Name), ListSortDirection.Ascending));
            SNTPServer = SNTPServers.SourceCollection.Cast<ServerInfoProfile>().FirstOrDefault(x => x.Name == SettingsManager.Current.SNTPLookup_SelectedSNTPServer.Name) ?? SNTPServers.SourceCollection.Cast<ServerInfoProfile>().First();

            LookupResultsView = CollectionViewSource.GetDefaultView(LookupResults);
            LookupResultsView.SortDescriptions.Add(new SortDescription(nameof(SNTPLookupResultInfo.Server), ListSortDirection.Ascending));

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {

        }

        #endregion

        #region ICommands & Actions
        public ICommand LookupCommand => new RelayCommand(p => LookupAction(), Lookup_CanExecute);

        private bool Lookup_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void LookupAction()
        {
            if (!IsLookupRunning)
                StartLookup();
        }

        public ICommand CopySelectedServerCommand => new RelayCommand(p => CopySelectedServerAction());


        private void CopySelectedServerAction()
        {
            ClipboardHelper.SetClipboard(SelectedLookupResult.Server);
        }

        public ICommand CopySelectedIPEndPointCommand => new RelayCommand(p => CopySelectedIPEndPointAction());

        private void CopySelectedIPEndPointAction()
        {
            ClipboardHelper.SetClipboard(SelectedLookupResult.IPEndPoint);
        }

        public ICommand CopySelectedNetworkTimeCommand => new RelayCommand(p => CopySelectedNetworkTimeAction());

        private void CopySelectedNetworkTimeAction()
        {
            ClipboardHelper.SetClipboard(SelectedLookupResult.DateTime.NetworkTime.ToString("yyyy.MM.dd HH:mm:ss.fff"));
        }

        public ICommand CopySelectedLocalStartTimeCommand => new RelayCommand(p => CopySelectedLocalStartTimeAction());

        private void CopySelectedLocalStartTimeAction()
        {
            ClipboardHelper.SetClipboard(SelectedLookupResult.DateTime.LocalStartTime.ToString("yyyy.MM.dd HH:mm:ss.fff"));
        }

        public ICommand CopySelectedLocalEndTimeCommand => new RelayCommand(p => CopySelectedLocalEndTimeAction());

        private void CopySelectedLocalEndTimeAction()
        {
            ClipboardHelper.SetClipboard(SelectedLookupResult.DateTime.LocalEndTime.ToString("yyyy.MM.dd HH:mm:ss.fff"));
        }

        public ICommand CopySelectedOffsetCommand => new RelayCommand(p => CopySelectedOffsetAction());

        private void CopySelectedOffsetAction()
        {
            ClipboardHelper.SetClipboard(SelectedLookupResult.DateTime.Offset.ToString() + " s");
        }

        public ICommand CopySelectedRoundTripDelayCommand => new RelayCommand(p => CopySelectedRoundTripDelayAction());

        private void CopySelectedRoundTripDelayAction()
        {
            ClipboardHelper.SetClipboard(SelectedLookupResult.DateTime.RoundTripDelay.ToString() + " ms");
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private async Task ExportAction()
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
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? LookupResults : new ObservableCollection<SNTPLookupResultInfo>(SelectedLookupResults.Cast<SNTPLookupResultInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.SNTPLookup_ExportFileType = instance.FileType;
                SettingsManager.Current.SNTPLookup_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.CSV, ExportManager.ExportFileType.XML, ExportManager.ExportFileType.JSON }, true, SettingsManager.Current.SNTPLookup_ExportFileType, SettingsManager.Current.SNTPLookup_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods      
        private void StartLookup()
        {
            IsStatusMessageDisplayed = false;
            StatusMessage = string.Empty;

            IsLookupRunning = true;

            // Reset the latest results
            LookupResults.Clear();

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == TabId).Header = SNTPServer.Name;
                }
            }

            SNTPLookupSettings settings = new()
            {
                Timeout = SettingsManager.Current.SNTPLookup_Timeout
            };

            SNTPLookup lookup = new(settings);

            lookup.ResultReceived += Lookup_ResultReceived;
            lookup.LookupError += Lookup_LookupError;
            lookup.LookupComplete += Lookup_LookupComplete;

            lookup.QueryAsync(SNTPServer.Servers);
        }
        
        public void OnClose()
        {

        }
        #endregion

        #region Events
        private void Lookup_ResultReceived(object sender, SNTPLookupResultArgs e)
        {
            var result = SNTPLookupResultInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                LookupResults.Add(result);
            }));
        }

        private void Lookup_LookupError(object sender, SNTPLookupErrorArgs e)
        {
            if (!string.IsNullOrEmpty(StatusMessage))
                StatusMessage += Environment.NewLine;

            StatusMessage += e.IsDNSError ? e.ErrorMessage : $"{e.Server} ({e.IPEndPoint}) ==> {e.ErrorMessage}";
            IsStatusMessageDisplayed = true;
        }

        private void Lookup_LookupComplete(object sender, EventArgs e)
        {
            IsLookupRunning = false;
        }
        #endregion
    }
}
