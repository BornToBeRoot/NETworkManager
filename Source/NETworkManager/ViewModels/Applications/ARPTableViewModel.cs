using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using NETworkManager.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NETworkManager.Collections;
using System.Net.NetworkInformation;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace NETworkManager.ViewModels.Applications
{
    public class ARPTableViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

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

            RefreshARPTableAction();

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {

        }
        #endregion

        #region ICommands & Actions
        public ICommand RefreshARPTableCommand
        {
            get { return new RelayCommand(p => RefreshARPTableAction()); }
        }

        private void RefreshARPTableAction()
        {
            ARPTable.Clear();

            Models.Network.ARPTable.GetARPTable().ForEach(x => ARPTable.Add(x));
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
    }
}