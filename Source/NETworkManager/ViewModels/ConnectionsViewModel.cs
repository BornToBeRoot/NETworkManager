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
        private readonly bool _isLoading;
        private readonly DispatcherTimer _autoRefreshTimer = new DispatcherTimer();

        private string _search;
        public string Search
        {
            get => _search;
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
            get => _connections;
            set
            {
                if (value == _connections)
                    return;

                _connections = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ConnectionsView { get; }

        private ConnectionInfo _selectedConnectionInfo;
        public ConnectionInfo SelectedConnectionInfo
        {
            get => _selectedConnectionInfo;
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
            get => _autoRefresh;
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
        public ConnectionsViewModel()
        {
            _isLoading = true;

            ConnectionsView = CollectionViewSource.GetDefaultView(Connections);
            ConnectionsView.SortDescriptions.Add(new SortDescription(nameof(ConnectionInfo.LocalIPAddressInt32), ListSortDirection.Ascending));
            ConnectionsView.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                var filter = Search.Replace(" ", "").Replace("-", "").Replace(":", "");

                // Search by local/remote IP Address, local/remote Port, Protocol and State
                return o is ConnectionInfo info && (info.LocalIPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.LocalPort.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.RemoteIPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.RemotePort.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.Protocol.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || LocalizationManager.GetStringByKey("String_TcpState_" + info.State.ToString()).IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1);
            };

            AutoRefreshTimes = CollectionViewSource.GetDefaultView(AutoRefreshTime.Defaults);
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

        public ICommand CopySelectedLocalIpAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedLocalIpAddressAction()); }
        }

        private void CopySelectedLocalIpAddressAction()
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

        public ICommand CopySelectedRemoteIpAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedRemoteIpAddressAction()); }
        }

        private void CopySelectedRemoteIpAddressAction()
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