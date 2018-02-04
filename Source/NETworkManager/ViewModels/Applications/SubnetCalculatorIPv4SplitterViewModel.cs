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
using MahApps.Metro.Controls.Dialogs;
using System.Linq;

namespace NETworkManager.ViewModels.Applications
{
    public class SubnetCalculatorIPv4SplitterViewModel : ViewModelBase
    {
        #region Variables
        CancellationTokenSource cancellationTokenSource;
        private IDialogCoordinator dialogCoordinator;

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

        private ICollectionView _newSubnetmaskOrCIDRHistoryView;
        public ICollectionView NewSubnetmaskOrCIDRHistoryView
        {
            get { return _newSubnetmaskOrCIDRHistoryView; }
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

        private SubnetInfo _selectedSplitResult;
        public SubnetInfo SelectedSplitResult
        {
            get { return _selectedSplitResult; }
            set
            {
                if (value == _selectedSplitResult)
                    return;

                _selectedSplitResult = value;
                OnPropertyChanged();
            }
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
        public SubnetCalculatorIPv4SplitterViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            // Set collection view
            _subnetHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_IPv4Splitter_SubnetHistory);
            _newSubnetmaskOrCIDRHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory);

            // Result view
            _splitResultsView = CollectionViewSource.GetDefaultView(SplitResult);
            _splitResultsView.SortDescriptions.Add(new SortDescription(nameof(SubnetInfo.NetworkAddressInt32), ListSortDirection.Ascending));
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

        public ICommand CopySelectedNetworkAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedNetworkAddressAction()); }
        }

        private void CopySelectedNetworkAddressAction()
        {
            Clipboard.SetText(SelectedSplitResult.NetworkAddress.ToString());
        }

        public ICommand CopySelectedBroadcastCommand
        {
            get { return new RelayCommand(p => CopySelectedBroadcastAction()); }
        }

        private void CopySelectedBroadcastAction()
        {
            Clipboard.SetText(SelectedSplitResult.Broadcast.ToString());
        }

        public ICommand CopySelectedIPAddressesCommand
        {
            get { return new RelayCommand(p => CopySelectedIPAddressesAction()); }
        }

        private void CopySelectedIPAddressesAction()
        {
            Clipboard.SetText(SelectedSplitResult.IPAddresses.ToString());
        }

        public ICommand CopySelectedSubnetmaskCommand
        {
            get { return new RelayCommand(p => CopySelectedSubnetmaskAction()); }
        }

        private void CopySelectedSubnetmaskAction()
        {
            Clipboard.SetText(SelectedSplitResult.Subnetmask.ToString());
        }

        public ICommand CopySelectedCIDRCommand
        {
            get { return new RelayCommand(p => CopySelectedCIDRAction()); }
        }

        private void CopySelectedCIDRAction()
        {
            Clipboard.SetText(SelectedSplitResult.CIDR.ToString());
        }

        public ICommand CopySelectedFirstIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedFirstIPAddressAction()); }
        }

        private void CopySelectedFirstIPAddressAction()
        {
            Clipboard.SetText(SelectedSplitResult.HostFirstIP.ToString());
        }

        public ICommand CopySelectedLastIPAddressCommand
        {
            get { return new RelayCommand(p => CopySelectedLastIPAddressAction()); }
        }

        private void CopySelectedLastIPAddressAction()
        {
            Clipboard.SetText(SelectedSplitResult.HostLastIP.ToString());
        }

        public ICommand CopySelectedHostCommand
        {
            get { return new RelayCommand(p => CopySelectedHostAction()); }
        }

        private void CopySelectedHostAction()
        {
            Clipboard.SetText(SelectedSplitResult.Hosts.ToString());
        }
        #endregion

        #region Methods
        private async void StartSplit()
        {
            DisplayStatusMessage = false;
            IsSplitRunning = true;

            SplitResult.Clear();

            string[] subnetSplit = Subnet.Trim().Split('/');
            string newSubnetmaskOrCidr = NewSubnetmaskOrCIDR.TrimStart('/');

            // Validate the user input and display warning
            double cidr = subnetSplit[1].Length < 3 ? double.Parse(subnetSplit[1]) : SubnetmaskHelper.ConvertSubnetmaskToCidr(subnetSplit[1]);
            double newCidr = newSubnetmaskOrCidr.Length < 3 ? double.Parse(newSubnetmaskOrCidr) : SubnetmaskHelper.ConvertSubnetmaskToCidr(newSubnetmaskOrCidr);

            if (65535 < (Math.Pow(2, (32 - cidr)) / Math.Pow(2, (32 - newCidr))))
            {
                MetroDialogSettings settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Application.Current.Resources["String_Button_Continue"] as string;
                settings.NegativeButtonText = Application.Current.Resources["String_Button_Cancel"] as string;

                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                if (await dialogCoordinator.ShowMessageAsync(this, Application.Current.Resources["String_Header_AreYouSure"] as string, Application.Current.Resources["String_TheProcessCanTakeUpSomeTimeAndResources"] as string, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                {
                    CancelSplit = false;
                    IsSplitRunning = false;

                    return;
                }
            }

            // Convert CIDR to subnetmask
            string subnetmask = Subnetmask.GetFromCidr((int)cidr).Subnetmask;
            string newSubnetmask = Subnetmask.GetFromCidr((int)newCidr).Subnetmask;

            // Add history
            AddSubnetToHistory(Subnet);
            AddNewSubnetmaskOrCIDRToHistory(NewSubnetmaskOrCIDR);

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

        private void AddSubnetToHistory(string subnet)
        {
            // Create the new list
            List<string> list = HistoryListHelper.Modify(SettingsManager.Current.SubnetCalculator_IPv4Splitter_SubnetHistory.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_IPv4Splitter_SubnetHistory.Clear();
            OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_IPv4Splitter_SubnetHistory.Add(x));
        }

        private void AddNewSubnetmaskOrCIDRToHistory(string newSubnetmaskOrCIDR)
        {
            // Create the new list
            List<string> list = HistoryListHelper.Modify(SettingsManager.Current.SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory.ToList(), newSubnetmaskOrCIDR, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory.Clear();
            OnPropertyChanged(nameof(NewSubnetmaskOrCIDR)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_IPv4Splitter_NewSubnetmaskOrCIDRHistory.Add(x));
        }

        public void OnShutdown()
        {
            if (IsSplitRunning)
                StopSplit();
        }
        #endregion
    }
}