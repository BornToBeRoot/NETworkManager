using NETworkManager.Model.Network;
using System.Net;
using System.Windows.Input;

namespace NETworkManager.ViewModels.Applications
{
    class SubnetCalculatorViewModel : ViewModelBase
    {
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

        private string _subnetmask;
        public string Subnetmask
        {
            get { return _subnetmask; }
            set
            {
                if (value == _subnetmask)
                    return;

                _subnetmask = value;
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

        public ICommand CalculateIPv4SubnetCommand
        {
            get { return new RelayCommand(p => CalculateIPv4SubnetAction()); }
        }

        private void CalculateIPv4SubnetAction()
        {
            SubnetInfo info = Subnet.CalculateIPv4Subnet(IPAddress.Parse(IPv4Address), IPAddress.Parse(Subnetmask));
                        
            DetailsNetworkAddress = info.NetworkAddress;
            DetailsBroadcast = info.Broadcast;
            DetailsSubnetmask = info.Subnetmask;
            DetailsCIDR = info.CIDR;
            DetailsTotalIPs = info.TotalIPs;
            DetailsHostIPRange = string.Format("{0} - {1}", info.HostFirstIP, info.HostLastIP);
            DetailsHostIPs = info.HostIPs;

            IsDetailsVisible = true;
        }
    }
}