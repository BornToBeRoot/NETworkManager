using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using NETworkManager.Helpers;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;

namespace NETworkManager.ViewModels.Applications
{
    public class SubnetCalculatorIPv4CalculatorViewModel : ViewModelBase
    {
        #region Variables
        private string _subnet;
        public string Subnet
        {
            get { return _subnet; }
            set
            {
                if (value == _subnet)
                    return;

                _subnet = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _subnetHistoryView;
        public ICollectionView SubnetHistoryView
        {
            get { return _subnetHistoryView; }
        }

        private bool _isDetailsVisible;
        public bool IsDetailsVisible
        {
            get { return _isDetailsVisible; }
            set
            {
                if (value == _isDetailsVisible)
                    return;


                _isDetailsVisible = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _detailsNetworkAddress;
        public IPAddress DetailsNetworkAddress
        {
            get { return _detailsNetworkAddress; }
            set
            {
                if (value == _detailsNetworkAddress)
                    return;

                _detailsNetworkAddress = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _detailsBroadcast;
        public IPAddress DetailsBroadcast
        {
            get { return _detailsBroadcast; }
            set
            {
                if (value == _detailsBroadcast)
                    return;

                _detailsBroadcast = value;
                OnPropertyChanged();
            }
        }

        private long _detailsIPAddresses;
        public long DetailsIPAddresses
        {
            get { return _detailsIPAddresses; }
            set
            {
                if (value == _detailsIPAddresses)
                    return;

                _detailsIPAddresses = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _detailsSubnetmask;
        public IPAddress DetailsSubnetmask
        {
            get { return _detailsSubnetmask; }
            set
            {
                if (value == _detailsSubnetmask)
                    return;

                _detailsSubnetmask = value;
                OnPropertyChanged();
            }
        }

        private int _detailsCIDR;
        public int DetailsCIDR
        {
            get { return _detailsCIDR; }
            set
            {
                if (value == _detailsCIDR)
                    return;

                _detailsCIDR = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _detailsFirstIPAddress;
        public IPAddress DetailsFirstIPAddress
        {
            get { return _detailsFirstIPAddress; }
            set
            {
                if (value == _detailsFirstIPAddress)
                    return;

                _detailsFirstIPAddress = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _detailsLastIPAddress;
        public IPAddress DetailsLastIPAddress
        {
            get { return _detailsLastIPAddress; }
            set
            {
                if (value == _detailsLastIPAddress)
                    return;

                _detailsLastIPAddress = value;
                OnPropertyChanged();
            }
        }

        private long _detailsHosts;
        public long DetailsHosts
        {
            get { return _detailsHosts; }
            set
            {
                if (value == _detailsHosts)
                    return;

                _detailsHosts = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public SubnetCalculatorIPv4CalculatorViewModel()
        {
            // Set collection view
            _subnetHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_IPv4Calculator_SubnetHistory);
        }
        #endregion

        #region ICommands
        public ICommand CalculateIPv4SubnetCommand
        {
            get { return new RelayCommand(p => CalculateIPv4SubnetAction()); }
        }
        #endregion

        #region Methods
        private void CalculateIPv4SubnetAction()
        {
            IsDetailsVisible = false;

            string[] subnet = Subnet.Trim().Split('/');

            string subnetmask = subnet[1];

            // Convert CIDR to subnetmask
            if (subnetmask.Length < 3)
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnet[1])).Subnetmask;

            SubnetInfo subnetInfo = Models.Network.Subnet.CalculateIPv4Subnet(IPAddress.Parse(subnet[0]), IPAddress.Parse(subnetmask));

            DetailsNetworkAddress = subnetInfo.NetworkAddress;
            DetailsBroadcast = subnetInfo.Broadcast;
            DetailsSubnetmask = subnetInfo.Subnetmask;
            DetailsCIDR = subnetInfo.CIDR;
            DetailsIPAddresses = subnetInfo.IPAddresses;
            DetailsFirstIPAddress = subnetInfo.HostFirstIP;
            DetailsLastIPAddress = subnetInfo.HostLastIP;
            DetailsHosts = subnetInfo.Hosts;

            IsDetailsVisible = true;

            AddSubnetToHistory(Subnet);
        }

        private void AddSubnetToHistory(string subnet)
        {
            // Create the new list
            List<string> list = HistoryListHelper.Modify(SettingsManager.Current.SubnetCalculator_IPv4Calculator_SubnetHistory.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_IPv4Calculator_SubnetHistory.Clear();
            OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_IPv4Calculator_SubnetHistory.Add(x));
        }
        #endregion
    }
}