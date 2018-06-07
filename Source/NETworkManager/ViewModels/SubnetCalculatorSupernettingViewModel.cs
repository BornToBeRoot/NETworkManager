using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Windows.Input;
using NETworkManager.Utilities;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;

namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorSupernettingViewModel : ViewModelBase
    {
        #region Variables
        private string _subnet1;
        public string Subnet1
        {
            get { return _subnet1; }
            set
            {
                if (value == _subnet1)
                    return;

                _subnet1 = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _subnet1HistoryView;
        public ICollectionView Subnet1HistoryView
        {
            get { return _subnet1HistoryView; }
        }

        private string _subnet2;
        public string Subnet2
        {
            get { return _subnet2; }
            set
            {
                if (value == _subnet2)
                    return;

                _subnet2 = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _subnet2HistoryView;
        public ICollectionView Subnet2HistoryView
        {
            get { return _subnet2HistoryView; }
        }

        private bool _isCalculationRunning;
        public bool IsCalculationRunning
        {
            get { return _isCalculationRunning; }
            set
            {
                if (value == _isCalculationRunning)
                    return;

                _isCalculationRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isResultVisible;
        public bool IsResultVisible
        {
            get { return _isResultVisible; }
            set
            {
                if (value == _isResultVisible)
                    return;


                _isResultVisible = value;
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
        public SubnetCalculatorSupernettingViewModel()
        {
            // Set collection view
            _subnet1HistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Supernetting_Subnet1);
            _subnet2HistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Supernetting_Subnet2);
        }
        #endregion

        #region ICommands & Actions
        public ICommand CalculateCommand
        {
            get { return new RelayCommand(p => CalculateAction()); }
        }

        private void CalculateAction()
        {
            Calculate();
        }
        #endregion

        #region Methods
        private void Calculate()
        {
            IsCalculationRunning = true;

            IPNetwork subnet1 = IPNetwork.Parse(Subnet1);
            IPNetwork subnet2 = IPNetwork.Parse(Subnet2);

            IPNetwork subnet = subnet1.Supernet(subnet2);

            NetworkAddress = subnet.Network;
            Broadcast = subnet.Broadcast;
            Subnetmask = subnet.Netmask;
            CIDR = subnet.Cidr;
            IPAddresses = subnet.Total;
            FirstIPAddress = subnet.FirstUsable;
            LastIPAddress = subnet.LastUsable;
            Hosts = subnet.Usable;

            IsResultVisible = true;

            AddSubnet1ToHistory(Subnet1);
            AddSubnet2ToHistory(Subnet2);

            IsCalculationRunning = false;
        }

        private void AddSubnet1ToHistory(string subnet)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Supernetting_Subnet1.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Supernetting_Subnet1.Clear();
            OnPropertyChanged(nameof(Subnet1)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Supernetting_Subnet1.Add(x));
        }

        private void AddSubnet2ToHistory(string subnet)
        {
            // Create the new list
            List<string> list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Supernetting_Subnet2.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Supernetting_Subnet2.Clear();
            OnPropertyChanged(nameof(Subnet2)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Supernetting_Subnet2.Add(x));
        }

        public void OnShutdown()
        {

        }
        #endregion
    }
}