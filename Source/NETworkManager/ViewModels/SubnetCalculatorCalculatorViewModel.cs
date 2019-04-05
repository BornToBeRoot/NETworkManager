using NETworkManager.Models.Settings;
using System.Net;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
using System.Numerics;
using System.Windows;
using MahApps.Metro.Controls;

namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorCalculatorViewModel : ViewModelBase
    {
        #region Variables
        private string _subnet;
        public string Subnet
        {
            get => _subnet;
            set
            {
                if (value == _subnet)
                    return;

                _subnet = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView SubnetHistoryView { get; }

        private bool _isResultVisible;
        public bool IsResultVisible
        {
            get => _isResultVisible;
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
            get => _networkAddress;
            set
            {
                if (Equals(value, _networkAddress))
                    return;

                _networkAddress = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _broadcast;
        public IPAddress Broadcast
        {
            get => _broadcast;
            set
            {
                if (Equals(value, _broadcast))
                    return;

                _broadcast = value;
                OnPropertyChanged();
            }
        }

        private BigInteger _ipAddresses;
        public BigInteger IPAddresses
        {
            get => _ipAddresses;
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
            get => _subnetmask;
            set
            {
                if (Equals(value, _subnetmask))
                    return;

                _subnetmask = value;
                OnPropertyChanged();
            }
        }

        private int _cidr;
        public int CIDR
        {
            get => _cidr;
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
            get => _firstIPAddress;
            set
            {
                if (Equals(value, _firstIPAddress))
                    return;

                _firstIPAddress = value;
                OnPropertyChanged();
            }
        }

        private IPAddress _lastIPAddress;
        public IPAddress LastIPAddress
        {
            get => _lastIPAddress;
            set
            {
                if (Equals(value, _lastIPAddress))
                    return;

                _lastIPAddress = value;
                OnPropertyChanged();
            }
        }

        private BigInteger _hosts;
        public BigInteger Hosts
        {
            get => _hosts;
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
            SubnetHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory);
        }
        #endregion

        #region ICommands
        public ICommand CalculateCommand => new RelayCommand(p => CalcualateAction(), Calculate_CanExecute);

        private bool Calculate_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void CalcualateAction()
        {
            Calculate();
        }
        #endregion

        #region Methods
        private void Calculate()
        {
            IsResultVisible = false;

            var subnet = IPNetwork.Parse(Subnet);

            NetworkAddress = subnet.Network;
            Broadcast = subnet.Broadcast;
            Subnetmask = subnet.Netmask;
            CIDR = subnet.Cidr;
            IPAddresses = subnet.Total;
            FirstIPAddress = subnet.FirstUsable;
            LastIPAddress = subnet.LastUsable;
            Hosts = subnet.Usable;
            
            IsResultVisible = true;

            AddSubnetToHistory(Subnet);
        }

        private void AddSubnetToHistory(string subnet)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.Clear();
            OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.Add(x));
        }
        #endregion
    }
}