using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
using System.Numerics;

namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorCalculatorViewModel : ViewModelBase
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

        private IPAddress _networkAddress;
        public IPAddress NetworkAddress
        {
            get { return _networkAddress; }
            set
            {
                if (value == _networkAddress)
                    return;

                _networkAddress = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _broadcast;
        public IPAddress Broadcast
        {
            get { return _broadcast; }
            set
            {
                if (value == _broadcast)
                    return;

                _broadcast = value;
                OnPropertyChanged();
            }
        }

        private BigInteger _ipAddresses;
        public BigInteger IPAddresses
        {
            get { return _ipAddresses; }
            set
            {
                if (value == _ipAddresses)
                    return;

                _ipAddresses = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _subnetmask;
        public IPAddress Subnetmask
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

        private int _cidr;
        public int CIDR
        {
            get { return _cidr; }
            set
            {
                if (value == _cidr)
                    return;

                _cidr = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _firstIPAddress;
        public IPAddress FirstIPAddress
        {
            get { return _firstIPAddress; }
            set
            {
                if (value == _firstIPAddress)
                    return;

                _firstIPAddress = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _lastIPAddress;
        public IPAddress LastIPAddress
        {
            get { return _lastIPAddress; }
            set
            {
                if (value == _lastIPAddress)
                    return;

                _lastIPAddress = value;
                OnPropertyChanged();
            }
        }

        private BigInteger _hosts;
        public BigInteger Hosts
        {
            get { return _hosts; }
            set
            {
                if (value == _hosts)
                    return;

                _hosts = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public SubnetCalculatorCalculatorViewModel()
        {
            // Set collection view
            _subnetHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory);
        }
        #endregion

        #region ICommands
        public ICommand CalculateCommand
        {
            get { return new RelayCommand(p => CalcualateAction()); }
        }

        private void CalcualateAction()
        {
            Calculate();
        }
        #endregion

        #region Methods
        private void Calculate()
        {
            IsDetailsVisible = false;

            IPNetwork network = IPNetwork.Parse(Subnet);

            NetworkAddress = network.Network;
            Broadcast = network.Broadcast;
            Subnetmask = network.Netmask;
            CIDR = network.Cidr;
            IPAddresses = network.Total;
            FirstIPAddress = network.FirstUsable;
            LastIPAddress = network.LastUsable;
            Hosts = network.Usable;
            
            IsDetailsVisible = true;

            AddSubnetToHistory(Subnet);
        }

        private void AddSubnetToHistory(string subnet)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.Clear();
            OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.Add(x));
        }
        #endregion
    }
}