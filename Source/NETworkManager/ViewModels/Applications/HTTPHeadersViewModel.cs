using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System;
using System.Windows.Input;
using System.Collections.Generic;
using NETworkManager.Models.Settings;
using System.Net;
using System.Windows.Threading;
using System.Diagnostics;

namespace NETworkManager.ViewModels.Applications
{
    public class HTTPHeadersViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        private string _websiteUri;
        public string WebsiteUri
        {
            get { return _websiteUri; }
            set
            {
                if (value == _websiteUri)
                    return;

                _websiteUri = value;
                OnPropertyChanged();
            }
        }

        private List<string> _websiteUriHistory = new List<string>();
        public List<string> WebsiteUriHistory
        {
            get { return _websiteUriHistory; }
            set
            {
                if (value == _websiteUriHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.HTTPHeader_WebsiteUriHistory = value;

                _websiteUriHistory = value;
                OnPropertyChanged();
            }
        }

        private bool _isCheckRunning;
        public bool IsCheckRunning
        {
            get { return _isCheckRunning; }
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
            get { return _headers; }
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
            get { return _displayStatusMessage; }
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
            get { return _statusMessage; }
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
            get { return _startTime; }
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
            get { return _duration; }
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
            get { return _endTime; }
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
            get { return _headersCount; }
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
            get { return _expandStatistics; }
            set
            {
                if (value == _expandStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.HTTPHeader_ExpandStatistics = value;

                _expandStatistics = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Contructor, load settings
        public HTTPHeadersViewModel()
        {            
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.HTTPHeader_WebsiteUriHistory != null)
                WebsiteUriHistory = new List<string>(SettingsManager.Current.HTTPHeader_WebsiteUriHistory);

            ExpandStatistics = SettingsManager.Current.HTTPHeader_ExpandStatistics;
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
            stopwatch.Start();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
            EndTime = null;

            Headers = null;
            HeadersCount = 0;

            try
            {
                WebHeaderCollection headers = await HTTPHeaders.GetHeadersAsync(new Uri(WebsiteUri));

                Headers = headers.ToString();
                HeadersCount = headers.Count;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
                DisplayStatusMessage = true;
            }

            WebsiteUriHistory = new List<string>(HistoryListHelper.Modify(WebsiteUriHistory, WebsiteUri, SettingsManager.Current.Application_HistoryListEntries));

            // Stop timer and stopwatch
            stopwatch.Stop();
            dispatcherTimer.Stop();

            Duration = stopwatch.Elapsed;
            EndTime = DateTime.Now;

            stopwatch.Reset();

            IsCheckRunning = false;
        }
        #endregion

        #region Events
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Duration = stopwatch.Elapsed;
        }
        #endregion
    }
}