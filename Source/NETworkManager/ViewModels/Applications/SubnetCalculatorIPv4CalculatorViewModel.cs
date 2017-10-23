using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using NETworkManager.Helpers;

namespace NETworkManager.ViewModels.Applications
{
    public class SubnetCalculatorIPv4CalculatorViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

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

        private List<string> _subnetHistory = new List<string>();
        public List<string> SubnetHistory
        {
            get { return _subnetHistory; }
            set
            {
                if (value == _subnetHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SubnetCalculator_IPv4Calculator_SubnetHistory = value;

                _subnetHistory = value;
                OnPropertyChanged();
            }
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

        private int _detailsIPAddresses;
        public int DetailsIPAddresses
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

        private IPAddress _detailsHostFirstIPAddress;
        public IPAddress DetailsHostFirstIPAddress
        {
            get { return _detailsHostFirstIPAddress; }
            set
            {
                if (value == _detailsHostFirstIPAddress)
                    return;

                _detailsHostFirstIPAddress = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _detailsHostLastIPAddress;
        public IPAddress DetailsHostLastIPAddress
        {
            get { return _detailsHostLastIPAddress; }
            set
            {
                if (value == _detailsHostLastIPAddress)
                    return;

                _detailsHostLastIPAddress = value;
                OnPropertyChanged();
            }
        }

        private int _detailsHosts;
        public int DetailsHosts
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
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.SubnetCalculator_IPv4Calculator_SubnetHistory != null)
                SubnetHistory = new List<string>(SettingsManager.Current.SubnetCalculator_IPv4Calculator_SubnetHistory);
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
            DetailsHostFirstIPAddress = subnetInfo.HostFirstIP;
            DetailsHostLastIPAddress = subnetInfo.HostLastIP;
            DetailsHosts = subnetInfo.Hosts;

            IsDetailsVisible = true;

            SubnetHistory = new List<string>(HistoryListHelper.Modify(SubnetHistory, Subnet, SettingsManager.Current.Application_HistoryListEntries));
        }
        #endregion
    }
}