using NETworkManager.Models.Network;
using NETworkManager.Models.Settings;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using NETworkManager.Helpers;
using System.Collections.ObjectModel;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;

namespace NETworkManager.ViewModels.Applications
{
    public class SubnetCalculatorIPv4SplitterViewModel : ViewModelBase
    {
        #region Variables
        CancellationTokenSource cancellationTokenSource;

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

        private bool _isSplitRunning;
        public bool IsSplitRunning
        {
            get { return _isSplitRunning; }
            set
            {
                if (value == _isSplitRunning)
                    return;

                _isSplitRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelSplit;
        public bool CancelSplit
        {
            get { return _cancelSplit; }
            set
            {
                if (value == _cancelSplit)
                    return;

                _cancelSplit = value;
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

        private ICollectionView _splitResultsView;
        public ICollectionView SplitResultView
        {
            get { return _splitResultsView; }
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

        #region Constructor, load settings
        public SubnetCalculatorIPv4SplitterViewModel()
        {
            LoadSettings();

            // Result view
            _splitResultsView = CollectionViewSource.GetDefaultView(SplitResult);
            _splitResultsView.SortDescriptions.Add(new SortDescription("NetworkAddressInt32", ListSortDirection.Ascending));

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

        #region ICommands & Actions
        public ICommand SplitIPv4SubnetCommand
        {
            get { return new RelayCommand(p => SplitIPv4SubnetAction()); }
        }

        private void SplitIPv4SubnetAction()
        {
            if (IsSplitRunning)
                StopSplit();
            else
                StartSplit();
        }
        #endregion

        #region Methods
        private async void StartSplit()
        {
            DisplayStatusMessage = false;
            IsSplitRunning = true;

            SplitResult.Clear();

            string[] subnetSplit = Subnet.Trim().Split('/');

            string subnetmask = subnetSplit[1];

            // Convert CIDR to subnetmask
            if (subnetmask.Length < 3)
                subnetmask = Subnetmask.GetFromCidr(int.Parse(subnetSplit[1])).Subnetmask;

            string newSubnetmask = NewSubnetmaskOrCIDR.TrimStart('/');

            if (newSubnetmask.Length < 3)
                newSubnetmask = Subnetmask.GetFromCidr(int.Parse(newSubnetmask)).Subnetmask;

            // Add history
            SubnetHistory = new List<string>(HistoryListHelper.Modify(SubnetHistory, Subnet, SettingsManager.Current.Application_HistoryListEntries));
            NewSubnetmaskOrCIDRHistory = new List<string>(HistoryListHelper.Modify(NewSubnetmaskOrCIDRHistory, NewSubnetmaskOrCIDR, SettingsManager.Current.Application_HistoryListEntries));

            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                foreach (SubnetInfo subnetInfo in await Models.Network.Subnet.SplitIPv4SubnetAsync(IPAddress.Parse(subnetSplit[0]), IPAddress.Parse(subnetmask), IPAddress.Parse(newSubnetmask), cancellationTokenSource.Token))
                    SplitResult.Add(subnetInfo);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = Application.Current.Resources["String_CanceledByUser"] as string;
                DisplayStatusMessage = true;
            }

            CancelSplit = false;
            IsSplitRunning = false;
        }

        private void StopSplit()
        {
            CancelSplit = true;
            cancellationTokenSource.Cancel();
        }

        public void OnShutdown()
        {
            if (IsSplitRunning)
                StopSplit();
        }
        #endregion
    }
}