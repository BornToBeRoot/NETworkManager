using NETworkManager.Models.Network;
using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Models.Settings;
using System.Windows.Threading;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class ConnectionsViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;
        private DispatcherTimer _autoRefreshTimer = new DispatcherTimer();

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                ConnectionsView.Refresh();

                OnPropertyChanged();
            }
        }

        private ObservableCollection<ConnectionInfo> _connections = new ObservableCollection<ConnectionInfo>();
        public ObservableCollection<ConnectionInfo> Connections
        {
            get { return _connections; }
            set
            {
                if (value == _connections)
                    return;

                _connections = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _connectionsView;
        public ICollectionView ConnectionsView
        {
            get { return _connectionsView; }
        }

        private ConnectionInfo _selectedConnectionInfo;
        public ConnectionInfo SelectedConnectionInfo
        {
            get { return _selectedConnectionInfo; }
            set
            {
                if (value == _selectedConnectionInfo)
                    return;

                _selectedConnectionInfo = value;
                OnPropertyChanged();
            }
        }

        private bool _autoRefresh;
        public bool AutoRefresh
        {
            get { return _autoRefresh; }
            set
            {
                if (value == _autoRefresh)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Connections_AutoRefresh = value;

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

        private ICollectionView _autoRefreshTimes;
        public ICollectionView AutoRefreshTimes
        {
            get { return _autoRefreshTimes; }
        }

        private AutoRefreshTimeInfo _selectedAutoRefreshTime;
        public AutoRefreshTimeInfo SelectedAutoRefreshTime
        {
            get { return _selectedAutoRefreshTime; }
            set
            {
                if (value == _selectedAutoRefreshTime)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Connections_AutoRefreshTime = value;

                _selectedAutoRefreshTime = value;

                if (AutoRefresh)
                    ChangeAutoRefreshTimerInterval(AutoRefreshTime.CalculateTimeSpan(value));

                OnPropertyChanged();
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
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
        #endregion

        #region Contructor, load settings
        public ConnectionsViewModel()
        {
            _connectionsView = CollectionViewSource.GetDefaultView(Connections);
            _connectionsView.SortDescriptions.Add(new SortDescription(nameof(ConnectionInfo.LocalIPAddressInt32), ListSortDirection.Ascending));
            _connectionsView.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                ConnectionInfo info = o as ConnectionInfo;

                string filter = Search.Replace(" ", "").Replace("-", "").Replace(":", "");

                // Search by local/remote IP Address, local/remote Port, Protocol and State
                return info.LocalIPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.LocalPort.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.RemoteIPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.RemotePort.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.Protocol.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || LocalizationManager.GetStringByKey("String_TcpState_" + info.State.ToString()).IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1;
            };

            _autoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.Defaults);
            SelectedAutoRefreshTime = AutoRefreshTimes.SourceCollection.Cast<AutoRefreshTimeInfo>().FirstOrDefault(x => (x.Value == SettingsManager.Current.Connections_AutoRefreshTime.Value && x.TimeUnit == SettingsManager.Current.Connections_AutoRefreshTime.TimeUnit));

            _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

            LoadSettings();

            _isLoading = false;

            Refresh();

            if (AutoRefresh)
                StartAutoRefreshTimer();
        }

        private void LoadSettings()
        {
            AutoRefresh = SettingsManager.Current.Connections_AutoRefresh;
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

        public ICommand CopySelectedLocalIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedLocalIPAddressAction()); }
        }

        private void CopySelectedLocalIPAddressAction()
        {
            Clipboard.SetText(SelectedConnectionInfo.LocalIPAddress.ToString());
        }

        public ICommand CopySelectedLocalPortCommand
        {
            get { return new RelayCommand(p => CopySelectedLocalPortAction()); }
        }

        private void CopySelectedLocalPortAction()
        {
            Clipboard.SetText(SelectedConnectionInfo.LocalPort.ToString());
        }

        public ICommand CopySelectedRemoteIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedRemoteIPAddressAction()); }
        }

        private void CopySelectedRemoteIPAddressAction()
        {
            Clipboard.SetText(SelectedConnectionInfo.RemoteIPAddress.ToString());
        }

        public ICommand CopySelectedRemotePortCommand
        {
            get { return new RelayCommand(p => CopySelectedRemotePortAction()); }
        }

        private void CopySelectedRemotePortAction()
        {
            Clipboard.SetText(SelectedConnectionInfo.RemotePort.ToString());
        }

        public ICommand CopySelectedProtocolCommand
        {
            get { return new RelayCommand(p => CopySelectedProtocolAction()); }
        }

        private void CopySelectedProtocolAction()
        {
            Clipboard.SetText(SelectedConnectionInfo.Protocol.ToString());
        }

        public ICommand CopySelectedStateCommand
        {
            get { return new RelayCommand(p => CopySelectedStateAction()); }
        }

        private void CopySelectedStateAction()
        {
            Clipboard.SetText(LocalizationManager.GetStringByKey("String_TcpState_" + SelectedConnectionInfo.State.ToString()));

        }
        #endregion

        #region Methods
        private async void Refresh()
        {
            IsRefreshing = true;

            Connections.Clear();

            (await Connection.GetActiveTcpConnectionsAsync()).ForEach(x => Connections.Add(x));

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
        #endregion

        #region Events
        private void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            Refresh();
        }
        #endregion
    }
}