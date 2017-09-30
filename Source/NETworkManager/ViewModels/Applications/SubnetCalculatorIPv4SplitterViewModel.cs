using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using NETworkManager.Helpers;
using System.Windows;
using System.Collections.ObjectModel;

namespace NETworkManager.ViewModels.Applications
{
    public class SubnetCalculatorIPv4SplitterViewModel : ViewModelBase
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
                    SettingsManager.Current.SubnetCalculator_IPv4Splitter_SubnetHistory = value;

                _subnetHistory = value;
                OnPropertyChanged();
            }
        }

        private string _newSubnetmaskOrCIDR;
        public string NewSubnetmaskOrCIDR
        {
            get { return _newSubnetmaskOrCIDR; }
            set
            {
                if (value == _newSubnetmaskOrCIDR)
                    return;

                _newSubnetmaskOrCIDR = value;
                OnPropertyChanged();
            }
        }

        private List<string> _newSubnetmaskOrCIDRHistory = new List<string>();
        public List<string> NewSubnetmaskOrCIDRHistory
        {
            get { return _newSubnetmaskOrCIDRHistory; }
            set
            {
                if (value == _newSubnetmaskOrCIDRHistory)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory = value;

                _newSubnetmaskOrCIDRHistory = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SubnetInfo> _splitResult = new ObservableCollection<SubnetInfo>();
        public ObservableCollection<SubnetInfo> SplitResult
        {
            get { return _splitResult; }
            set
            {
                if (value == _splitResult)
                    return;

                _splitResult = value;
            }
        }
        #endregion

        #region Constructor, load settings
        public SubnetCalculatorIPv4SplitterViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            if (SettingsManager.Current.SubnetCalculator_IPv4Splitter_SubnetHistory != null)
                SubnetHistory = new List<string>(SettingsManager.Current.SubnetCalculator_IPv4Splitter_SubnetHistory);

            if (SettingsManager.Current.SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory != null)
                NewSubnetmaskOrCIDRHistory = new List<string>(SettingsManager.Current.SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory);
        }
        #endregion

        #region ICommands
        public ICommand SplitIPv4SubnetCommand
        {
            get { return new RelayCommand(p => SplitIPv4SubnetAction()); }
        }
        #endregion

        #region Methods
        private void SplitIPv4SubnetAction()
        {
            string[] subnet = Subnet.Trim().Split('/');

            string subnetmask = subnet[1];

            // Convert CIDR to subnetmask
            if (subnetmask.Length < 3)
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnet[1])).Subnetmask;

            SubnetInfo subnetInfo = Models.Network.Subnet.CalculateIPv4Subnet(IPAddress.Parse(subnet[0]), IPAddress.Parse(subnetmask));
            
            SubnetHistory = new List<string>(HistoryListHelper.Modify(SubnetHistory, Subnet, SettingsManager.Current.Application_HistoryListEntries));
            NewSubnetmaskOrCIDRHistory = new List<string>(HistoryListHelper.Modify(NewSubnetmaskOrCIDRHistory, NewSubnetmaskOrCIDR, SettingsManager.Current.Application_HistoryListEntries));
        }
        #endregion
    }
}