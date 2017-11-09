using NETworkManager.Models.Network;
using NETworkManager.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace NETworkManager.ViewModels.Applications
{
    public class ARPTableViewModel : ViewModelBase
    {
        #region Variables
        //private bool _isLoading = true;

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                if (value == _filter)
                    return;

                _filter = value;

                ARPTableView.Refresh();

                OnPropertyChanged();
            }
        }

        private ObservableCollection<ARPTableInfo> _arpTable = new ObservableCollection<ARPTableInfo>();
        public ObservableCollection<ARPTableInfo> ARPTable
        {
            get { return _arpTable; }
            set
            {
                if (value == _arpTable)
                    return;

                _arpTable = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _arpTableView;
        public ICollectionView ARPTableView
        {
            get { return _arpTableView; }
        }

        private ARPTableInfo _selectedARPTableInfo;
        public ARPTableInfo SelectedARPTableInfo
        {
            get { return _selectedARPTableInfo; }
            set
            {
                if (value == _selectedARPTableInfo)
                    return;

                _selectedARPTableInfo = value;
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
        public ARPTableViewModel()
        {
            _arpTableView = CollectionViewSource.GetDefaultView(ARPTable);
            _arpTableView.SortDescriptions.Add(new SortDescription("IPAddressInt32", ListSortDirection.Ascending));
            _arpTableView.Filter = o =>
            {
                if (string.IsNullOrEmpty(Filter))
                    return true;

                ARPTableInfo info = o as ARPTableInfo;

                string filter = Filter.Replace(" ", "").Replace("-", "").Replace(":", "");

                // Search by IPAddress and MACAddress
                return info.IPAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || info.MACAddress.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1;
            };

            Refresh();

            LoadSettings();

            //_isLoading = false;
        }

        private void LoadSettings()
        {

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

        public ICommand DeleteCommand
        {
            get { return new RelayCommand(p => DeleteAction()); }
        }

        private async void DeleteAction()
        {
            DisplayStatusMessage = false;

            try
            {
                ARPTable arpTable = new ARPTable();

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

        public ICommand CopySelectedIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressAction()); }
        }

        private void CopySelectedIPAddressAction()
        {
            Clipboard.SetText(SelectedARPTableInfo.IPAddress.ToString());
        }

        public ICommand CopySelectedMACAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedMACAddressAction()); }
        }

        private void CopySelectedMACAddressAction()
        {
            Clipboard.SetText(MACAddressHelper.GetDefaultFormat(SelectedARPTableInfo.MACAddress.ToString()));
        }
        #endregion

        #region Methods
        private void Refresh()
        {
            ARPTable.Clear();

            Models.Network.ARPTable.GetTable().ForEach(x => ARPTable.Add(x));
        }
        #endregion

        #region Events
        private void ArpTable_UserHasCanceled(object sender, EventArgs e)
        {
            StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
            DisplayStatusMessage = true;
        }
        #endregion
    }
}