using NETworkManager.Models.Network;
using System;
using System.Windows.Input;
using NETworkManager.Settings;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Controls;
using Dragablz;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Export;
using NETworkManager.Localization.Resources;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
    public class WhoisViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;
        
        public readonly int TabId;
        private bool _firstLoad = true;

        private readonly bool _isLoading;

        private string _domain;
        public string Domain
        {
            get => _domain;
            set
            {
                if (value == _domain)
                    return;

                _domain = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView WebsiteUriHistoryView { get; }

        private bool _isWhoisRunning;
        public bool IsWhoisRunning
        {
            get => _isWhoisRunning;
            set
            {
                if (value == _isWhoisRunning)
                    return;

                _isWhoisRunning = value;
                OnPropertyChanged();
            }
        }

        private string _whoisResult;
        public string WhoisResult
        {
            get => _whoisResult;
            set
            {
                if (value == _whoisResult)
                    return;

                _whoisResult = value;
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
        public WhoisViewModel(IDialogCoordinator instance ,int tabId, string domain)
        {
            _isLoading = true;

            _dialogCoordinator = instance;

            TabId = tabId;
            Domain = domain;

            // Set collection view
            WebsiteUriHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Whois_DomainHistory);

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        public void OnLoaded()
        {
            if (!_firstLoad)
                return;

            if (!string.IsNullOrEmpty(Domain))
                Query();

            _firstLoad = false;
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions
        public ICommand QueryCommand => new RelayCommand(p => QueryAction(), Query_CanExecute);

        private bool Query_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void QueryAction()
        {
            Query();
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, WhoisResult);
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Strings.Error, Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.Whois_ExportFileType = instance.FileType;
                SettingsManager.Current.Whois_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.TXT}, false, SettingsManager.Current.Whois_ExportFileType, SettingsManager.Current.Whois_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private async void Query()
        {
            DisplayStatusMessage = false;
            IsWhoisRunning = true;

            WhoisResult = null;

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == TabId).Header = Domain;
                }
            }

            try
            {
                var whoisServer = Whois.GetWhoisServer(Domain);

                if (string.IsNullOrEmpty(whoisServer))
                {
                    StatusMessage = string.Format(Strings.WhoisServerNotFoundForTheDomain, Domain);
                    DisplayStatusMessage = true;
                }
                else
                {
                    WhoisResult = await Whois.QueryAsync(Domain, whoisServer);

                    AddDomainToHistory(Domain);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }

            IsWhoisRunning = false;
        }

        public void OnClose()
        {

        }

        private void AddDomainToHistory(string websiteUri)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.Whois_DomainHistory.ToList(), websiteUri, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.Whois_DomainHistory.Clear();
            OnPropertyChanged(nameof(Domain)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.Whois_DomainHistory.Add(x));
        }
        #endregion

        #region Events    
        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
        }
        #endregion
    }
}