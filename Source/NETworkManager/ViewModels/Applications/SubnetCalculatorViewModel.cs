using NETworkManager.Models.Network;
using System.Net;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Applications
{
    public class SubnetCalculatorViewModel : ViewModelBase
    {
        #region Variables
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

        private string _ipv4Address;

        public string IPv4Address
        {
            get { return _ipv4Address; }
            set
            {
                if (value == _ipv4Address)
                    return;

                _ipv4Address = value;
                OnPropertyChanged();
            }
        }

        private string _subnetmaskOrCidr;
        public string SubnetmaskOrCidr
        {
            get { return _subnetmaskOrCidr; }
            set
            {
                if (value == _subnetmaskOrCidr)
                    return;

                _subnetmaskOrCidr = value;
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

        private int _detailsTotalIPs;
        public int DetailsTotalIPs
        {
            get { return _detailsTotalIPs; }
            set
            {
                if (value == _detailsTotalIPs)
                    return;

                _detailsTotalIPs = value;
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

        private string _detailsHostIPRange;
        public string DetailsHostIPRange
        {
            get { return _detailsHostIPRange; }
            set
            {
                if (value == _detailsHostIPRange)
                    return;

                _detailsHostIPRange = value;
                OnPropertyChanged();
            }
        }

        private int _detailsHostIPs;
        public int DetailsHostIPs
        {
            get { return _detailsHostIPs; }
            set
            {
                if (value == _detailsHostIPs)
                    return;

                _detailsHostIPs = value;
                OnPropertyChanged();
            }
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
            string subnetmask = SubnetmaskOrCidr;

            if (SubnetmaskOrCidr.StartsWith("/"))
                subnetmask = Subnetmask.GetFromCidr(int.Parse(SubnetmaskOrCidr.TrimStart('/'))).Subnetmask;

            SubnetInfo subnetInfo = Subnet.CalculateIPv4Subnet(IPAddress.Parse(IPv4Address), IPAddress.Parse(subnetmask));

            DetailsNetworkAddress = subnetInfo.NetworkAddress;
            DetailsBroadcast = subnetInfo.Broadcast;
            DetailsSubnetmask = subnetInfo.Subnetmask;
            DetailsCIDR = subnetInfo.CIDR;
            DetailsTotalIPs = subnetInfo.TotalIPs;
            DetailsHostIPRange = string.Format("{0} - {1}", subnetInfo.HostFirstIP, subnetInfo.HostLastIP);
            DetailsHostIPs = subnetInfo.HostIPs;

            IsDetailsVisible = true;
        }
        #endregion
    }
}