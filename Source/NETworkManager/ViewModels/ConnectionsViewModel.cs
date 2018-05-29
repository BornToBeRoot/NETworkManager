using NETworkManager.Models.Network;
using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using NETworkManager.Utilities;
using System.Windows;
using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels
{
    public class ConnectionsViewModel : ViewModelBase
    {
        #region Variables
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
            _connectionsView.SortDescriptions.Add(new SortDescription(nameof(ARPTableInfo.IPAddressInt32), ListSortDirection.Ascending));
            _connectionsView.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                ConnectionInfo info = o as ConnectionInfo;

                string filter = Search.Replace(" ", "").Replace("-", "").Replace(":", "");

                // Search by IPAddress and MACAddress
                return info.LocalIPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.LocalPort.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.RemoteIPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.RemotePort.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.State.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1;
            };

            Refresh();
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

            (await Connection.GetActiveTCPConnectionsAsync()).ForEach(x => Connections.Add(x));

            IsRefreshing = false;
        }
        #endregion
    }
}