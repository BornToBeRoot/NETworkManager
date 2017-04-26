using NETworkManager.Settings;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using NETworkManager.Model.Network;
using NETworkManager.Utilities.Network;
using NETworkManager.Utilities.Common;
using NETworkManager.Model.Common;

namespace NETworkManager.ViewModels.Applications
{
    class IPScannerViewModel : ViewModelBase
    {
        #region Variables
        private IDialogCoordinator dialogCoordinator;
        MetroDialogSettings dialogSettings = new MetroDialogSettings();

        CancellationTokenSource cancellationTokenSource;

        private bool _isLoading = true;

        private string _ipRange;
        public string IPRange
        {
            get { return _ipRange; }
            set
            {
                if (value == _ipRange)
                    return;

                _ipRange = value;
                OnPropertyChanged();
            }
        }

        private List<string> _ipRangeHistory = new List<string>();
        public List<string> IPRangeHistory
        {
            get { return _ipRangeHistory; }
            set
            {
                if (value == _ipRangeHistory)
                    return;

                if (!_isLoading)
                {
                    StringCollection collection = new StringCollection();
                    collection.AddRange(value.ToArray());

                    Properties.Settings.Default.IPScanner_IPRangeHistory = collection;

                    SettingsManager.SettingsChanged = true;
                }

                _ipRangeHistory = value;
                OnPropertyChanged();
            }
        }

        private int _threads;
        public int Threads
        {
            get { return _threads; }
            set
            {
                if (value == _threads)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.IPScanner_ConcurrentThreads = value;

                    SettingsManager.SettingsChanged = true;
                }

                _threads = value;
                OnPropertyChanged();
            }
        }

        private int _timeout;
        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.IPScanner_Timeout = value;

                    SettingsManager.SettingsChanged = true;
                }

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private int _buffer;
        public int Buffer
        {
            get { return _buffer; }
            set
            {
                if (value == _buffer)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.IPScanner_Buffer = value;

                    SettingsManager.SettingsChanged = true;
                }

                _buffer = value;
                OnPropertyChanged();
            }
        }

        private int _Attempts;
        public int Attempts
        {
            get { return _Attempts; }
            set
            {
                if (value == _Attempts)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.IPScanner_Attempts = value;

                    SettingsManager.SettingsChanged = true;
                }

                _Attempts = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveHostname;
        public bool ResolveHostname
        {
            get { return _resolveHostname; }
            set
            {
                if (value == _resolveHostname)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.IPScanner_ResolveHostname = value;

                    SettingsManager.SettingsChanged = true;
                }

                _resolveHostname = value;
                OnPropertyChanged();
            }
        }

        private bool _resolveMACAddress;
        public bool ResolveMACAddress
        {
            get { return _resolveMACAddress; }
            set
            {
                if (value == _resolveMACAddress)
                    return;

                if (!_isLoading)
                {
                    Properties.Settings.Default.IPScanner_ResolveMACAddress = value;

                    SettingsManager.SettingsChanged = true;
                }

                _resolveMACAddress = value;
                OnPropertyChanged();
            }
        }

        private bool _isScanRunning;
        public bool IsScanRunning
        {
            get { return _isScanRunning; }
            set
            {
                if (value == _isScanRunning)
                    return;

                _isScanRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelScan;
        public bool CancelScan
        {
            get { return _cancelScan; }
            set
            {
                if (value == _cancelScan)
                    return;

                _cancelScan = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<HostInfo> _ipScanResult = new ObservableCollection<HostInfo>();
        public ObservableCollection<HostInfo> IPScanResult
        {
            get { return _ipScanResult; }
            set
            {
                if (value == _ipScanResult)
                    return;

                _ipScanResult = value;
            }
        }

        private int _progressBarMaximum;
        public int ProgressBarMaximum
        {
            get { return _progressBarMaximum; }
            set
            {
                if (value == _progressBarMaximum)
                    return;

                _progressBarMaximum = value;
                OnPropertyChanged();
            }
        }

        private int _progressBarValue;
        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                if (value == _progressBarValue)
                    return;

                _progressBarValue = value;
                OnPropertyChanged();
            }
        }

        private bool _preparingScan;
        public bool PreparingScan
        {
            get { return _preparingScan; }
            set
            {
                if (value == _preparingScan)
                    return;

                _preparingScan = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public IPScannerViewModel(IDialogCoordinator instance)
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

        #region Settings
        private void LoadSettings()
        {
            if (Properties.Settings.Default.IPScanner_IPRangeHistory != null)
                IPRangeHistory = new List<string>(Properties.Settings.Default.IPScanner_IPRangeHistory.Cast<string>().ToList());

            Timeout = Properties.Settings.Default.IPScanner_Timeout;
            Buffer = Properties.Settings.Default.IPScanner_Buffer;
            Attempts = Properties.Settings.Default.IPScanner_Attempts;
            Threads = Properties.Settings.Default.IPScanner_ConcurrentThreads;
            ResolveHostname = Properties.Settings.Default.IPScanner_ResolveHostname;
            ResolveMACAddress = Properties.Settings.Default.IPScanner_ResolveMACAddress;
        }
        #endregion

        #region ICommands
        public ICommand ScanCommand
        {
            get { return new RelayCommand(p => ScanAction()); }
        }
        #endregion

        #region Methods
        private void ScanAction()
        {
            if (IsScanRunning)
                StopScan();
            else
                StartScan();
        }

        private async void StartScan()
        {
            IsScanRunning = true;
            PreparingScan = true;

            IPScanResult.Clear();

            IPAddress[] ipAddresses;

            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Create a list of all ip addresses
                ipAddresses = await IPScanRangeHelper.ConvertIPRangeToIPAddressArrayAsync(IPRange, cancellationTokenSource.Token); //IPScanRangeHelper.ConvertIPRangeToIPAddressArrayAsync(IPRange, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                UserHasCanceled(this, EventArgs.Empty);
                return;
            }

            ProgressBarMaximum = ipAddresses.Length;
            ProgressBarValue = 0;

            PreparingScan = false;

            // Add the range to the history
            IPRangeHistory = new List<string>(HistoryListHelper.Modify(IPRangeHistory, IPRange, 5));

            IPScannerOptions ipScannerOptions = new IPScannerOptions
            {
                Threads = Threads,
                Timeout = Timeout,
                Buffer = new byte[Buffer],
                Attempts = Attempts,
                ResolveHostname = ResolveHostname,
                ResolveMACAddress = ResolveMACAddress
            };

            IPScanner ipScanner = new IPScanner();

            ipScanner.HostFound += IpScanner_HostFound;
            ipScanner.ScanComplete += IpScanner_ScanComplete;
            ipScanner.ProgressChanged += IpScanner_ProgressChanged;
            ipScanner.UserHasCanceled += UserHasCanceled;

            ipScanner.ScanAsync(ipAddresses, ipScannerOptions, cancellationTokenSource.Token);            
        }

        private void StopScan()
        {
            CancelScan = true;
            cancellationTokenSource.Cancel();
        }
        #endregion

        #region Events
        private void IpScanner_HostFound(object sender, HostFoundArgs e)
        {
            HostInfo ipScannerInfo = HostInfo.Parse(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                IPScanResult.Add(ipScannerInfo);
            }));
        }

        private void IpScanner_ScanComplete(object sender, EventArgs e)
        {
            IsScanRunning = false;
        }

        private void IpScanner_ProgressChanged(object sender, ProgressChangedArgs e)
        {
            ProgressBarValue = e.Value;
        }

        private async void UserHasCanceled(object sender, EventArgs e)
        {
            CancelScan = false;
            IsScanRunning = false;

            await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_CanceledByUser"] as string, Application.Current.Resources["String_CanceledByUserMessage"] as string, MessageDialogStyle.Affirmative, dialogSettings);
        }
        #endregion

        #region OnShutdown
        public void OnShutdown()
        {
            if (IsScanRunning)
                StopScan();
        }
        #endregion
    }
}
