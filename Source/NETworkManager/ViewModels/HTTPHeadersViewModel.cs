using NETworkManager.Models.Network;
using System;
using System.Windows.Input;
using NETworkManager.Models.Settings;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Controls;
using Dragablz;

namespace NETworkManager.ViewModels
{
    public class HTTPHeadersViewModel : ViewModelBase
    {
        #region Variables
        private readonly int _tabId;
        private bool _firstLoad = true;

        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly bool _isLoading;

        private string _websiteUri;
        public string WebsiteUri
        {
            get => _websiteUri;
            set
            {
                if (value == _websiteUri)
                    return;

                _websiteUri = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView WebsiteUriHistoryView { get; }

        private bool _isCheckRunning;
        public bool IsCheckRunning
        {
            get => _isCheckRunning;
            set
            {
                if (value == _isCheckRunning)
                    return;

                _isCheckRunning = value;
                OnPropertyChanged();
            }
        }

        private string _headers;
        public string Headers
        {
            get => _headers;
            set
            {
                if (value == _headers)
                    return;

                _headers = value;
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

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get => _startTime;
            set
            {
                if (value == _startTime)
                    return;

                _startTime = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value == _duration)
                    return;

                _duration = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get => _endTime;
            set
            {
                if (value == _endTime)
                    return;

                _endTime = value;
                OnPropertyChanged();
            }
        }

        private int _headersCount;
        public int HeadersCount
        {
            get => _headersCount;
            set
            {
                if (value == _headersCount)
                    return;

                _headersCount = value;
                OnPropertyChanged();
            }
        }

        private bool _expandStatistics;
        public bool ExpandStatistics
        {
            get => _expandStatistics;
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.HTTPHeaders_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        public bool ShowStatistics => SettingsManager.Current.HTTPHeaders_ShowStatistics;

        #endregion

        #region Contructor, load settings
        public HTTPHeadersViewModel(int tabId, string websiteUri)
        {
            _isLoading = true;

            _tabId = tabId;
            WebsiteUri = websiteUri;

            // Set collection view
            WebsiteUriHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.HTTPHeaders_WebsiteUriHistory);

            LoadSettings();

            // Detect if settings have changed...
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            _isLoading = false;
        }

        public void OnLoaded()
        {
            if(!_firstLoad)
                return;

            if(!string.IsNullOrEmpty(WebsiteUri))
                Check();
            
            _firstLoad = false;
        }

        private void LoadSettings()
        {
            ExpandStatistics = SettingsManager.Current.HTTPHeaders_ExpandStatistics;
        }
        #endregion

        #region ICommands & Actions
        public ICommand CheckCommand
        {
            get { return new RelayCommand(p => CheckAction()); }
        }

        private void CheckAction()
        {
            Check();
        }
        #endregion

        #region Methods
        private async void Check()
        {
            DisplayStatusMessage = false;
            IsCheckRunning = true;

            // Measure time
            StartTime = DateTime.Now;
            _stopwatch.Start();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Start();
            EndTime = null;

            Headers = null;
            HeadersCount = 0;

            // Change the tab title (not nice, but it works)
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (window != null)
            {
                foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
                {
                    tabablzControl.Items.OfType<DragablzTabItem>().First(x => x.Id == _tabId).Header = WebsiteUri;
                }
            }

            try
            {
                var options = new HTTPHeadersOptions()
                {
                    Timeout = SettingsManager.Current.HTTPHeaders_Timeout
                };

                var headers = await HTTPHeaders.GetHeadersAsync(new Uri(WebsiteUri), options);

                Headers = headers.ToString();
                HeadersCount = headers.Count;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }

            AddWebsiteUriToHistory(WebsiteUri);

            // Stop timer and stopwatch
            _stopwatch.Stop();
            _dispatcherTimer.Stop();

            Duration = _stopwatch.Elapsed;
            EndTime = DateTime.Now;

            _stopwatch.Reset();

            IsCheckRunning = false;
        }

        public void OnClose()
        {

        }

        private void AddWebsiteUriToHistory(string websiteUri)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.HTTPHeaders_WebsiteUriHistory.ToList(), websiteUri, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.HTTPHeaders_WebsiteUriHistory.Clear();
            OnPropertyChanged(nameof(WebsiteUri)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.HTTPHeaders_WebsiteUriHistory.Add(x));
        }
        #endregion

        #region Events
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = _stopwatch.Elapsed;
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsInfo.HTTPHeaders_ShowStatistics))
                OnPropertyChanged(nameof(ShowStatistics));
        }
        #endregion
    }
}