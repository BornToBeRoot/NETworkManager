using NETworkManager.Settings;
using System.Windows.Input;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System.Linq;
using System.Net;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Views;
using System.Text.RegularExpressions;

namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorSubnettingViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

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

        private string _newSubnetmaskOrCIDR;
        public string NewSubnetmaskOrCIDR
        {
            get => _newSubnetmaskOrCIDR;
            set
            {
                if (value == _newSubnetmaskOrCIDR)
                    return;

                _newSubnetmaskOrCIDR = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView NewSubnetmaskOrCIDRHistoryView { get; }

        private bool _isCalculationRunning;
        public bool IsCalculationRunning
        {
            get => _isCalculationRunning;
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
            get => _isResultVisible;
            set
            {
                if (value == _isResultVisible)
                    return;


                _isResultVisible = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IPNetworkInfo> _subnetsResult = new ObservableCollection<IPNetworkInfo>();
        public ObservableCollection<IPNetworkInfo> SubnetsResult
        {
            get => _subnetsResult;
            set
            {
                if (value == _subnetsResult)
                    return;

                _subnetsResult = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView SubnetsResultsView { get; }

        private IPNetworkInfo _selectedSubnetResult;
        public IPNetworkInfo SelectedSubnetResult
        {
            get => _selectedSubnetResult;
            set
            {
                if (value == _selectedSubnetResult)
                    return;

                _selectedSubnetResult = value;
                OnPropertyChanged();
            }
        }


        private IList _selectedSubnetResults = new ArrayList();
        public IList SelectedSubnetResults
        {
            get => _selectedSubnetResults;
            set
            {
                if (Equals(value, _selectedSubnetResults))
                    return;

                _selectedSubnetResults = value;
                OnPropertyChanged();
            }
        }


        private bool _displayStatusMessage;
        public bool DisplayStatusMessage
        {
            get => _displayStatusMessage;
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
            get => _statusMessage;
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
        public SubnetCalculatorSubnettingViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            // Set collection view
            SubnetHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory);
            NewSubnetmaskOrCIDRHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory);

            // Result view
            SubnetsResultsView = CollectionViewSource.GetDefaultView(SubnetsResult);
        }
        #endregion

        #region ICommands & Actions
        public ICommand CalculateCommand => new RelayCommand(p => CalculateAction(), Calculate_CanExecute);

        private bool Calculate_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void CalculateAction()
        {
            Calculate();
        }

        public ICommand CopySelectedNetworkAddressCommand => new RelayCommand(p => CopySelectedNetworkAddressAction());

        private void CopySelectedNetworkAddressAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.Network.ToString());
        }

        public ICommand CopySelectedBroadcastCommand => new RelayCommand(p => CopySelectedBroadcastAction());

        private void CopySelectedBroadcastAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.Broadcast.ToString());
        }

        public ICommand CopySelectedIPAddressesCommand => new RelayCommand(p => CopySelectedIPAddressesAction());

        private void CopySelectedIPAddressesAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.Total.ToString());
        }

        public ICommand CopySelectedSubnetmaskCommand => new RelayCommand(p => CopySelectedSubnetmaskAction());

        private void CopySelectedSubnetmaskAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.Netmask.ToString());
        }

        public ICommand CopySelectedCIDRCommand => new RelayCommand(p => CopySelectedCIDRAction());

        private void CopySelectedCIDRAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.Cidr.ToString());
        }

        public ICommand CopySelectedFirstIPAddressCommand => new RelayCommand(p => CopySelectedFirstIPAddressAction());

        private void CopySelectedFirstIPAddressAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.FirstUsable.ToString());
        }

        public ICommand CopySelectedLastIPAddressCommand => new RelayCommand(p => CopySelectedLastIPAddressAction());

        private void CopySelectedLastIPAddressAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.LastUsable.ToString());
        }

        public ICommand CopySelectedHostCommand => new RelayCommand(p => CopySelectedHostAction());

        private void CopySelectedHostAction()
        {
            ClipboardHelper.SetClipboard(SelectedSubnetResult.Usable.ToString());
        }

        public ICommand ExportCommand => new RelayCommand(p => ExportAction());

        private async void ExportAction()
        {
            var customDialog = new CustomDialog
            {
                Title = Localization.Resources.Strings.Export
            };

            var exportViewModel = new ExportViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                try
                {
                    ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? SubnetsResult : new ObservableCollection<IPNetworkInfo>(SelectedSubnetResults.Cast<IPNetworkInfo>().ToArray()));
                }
                catch (Exception ex)
                {
                    var settings = AppearanceManager.MetroDialog;
                    settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                    await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
                }

                SettingsManager.Current.SubnetCalculator_Subnetting_ExportFileType = instance.FileType;
                SettingsManager.Current.SubnetCalculator_Subnetting_ExportFilePath = instance.FilePath;
            }, instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, new ExportManager.ExportFileType[] { ExportManager.ExportFileType.CSV, ExportManager.ExportFileType.XML, ExportManager.ExportFileType.JSON }, true, SettingsManager.Current.SubnetCalculator_Subnetting_ExportFileType, SettingsManager.Current.SubnetCalculator_Subnetting_ExportFilePath);

            customDialog.Content = new ExportDialog
            {
                DataContext = exportViewModel
            };

            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Methods
        private async void Calculate()
        {
            DisplayStatusMessage = false;
            IsCalculationRunning = true;

            SubnetsResult.Clear();

            var subnet = IPNetwork.Parse(Subnet);

            byte newCidr = 0;

            // Support subnetmask like 255.255.255.0
            if (Regex.IsMatch(NewSubnetmaskOrCIDR, RegexHelper.SubnetmaskRegex))
                newCidr = Convert.ToByte(Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(NewSubnetmaskOrCIDR)));
            else
                newCidr = Convert.ToByte(NewSubnetmaskOrCIDR.TrimStart('/'));

            // Ask the user if there is a large calculation...
            var baseCidr = subnet.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;

            if (65535 < Math.Pow(2, baseCidr - subnet.Cidr) / Math.Pow(2, (baseCidr - newCidr)))
            {
                var settings = AppearanceManager.MetroDialog;

                settings.AffirmativeButtonText = Localization.Resources.Strings.Continue;
                settings.NegativeButtonText = Localization.Resources.Strings.Cancel;

                settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

                if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.AreYouSure, Localization.Resources.Strings.TheProcessCanTakeUpSomeTimeAndResources, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                {
                    IsCalculationRunning = false;

                    return;
                }
            }

            // This still slows the application / freezes the ui... there are to many updates to the ui thread...
            await Task.Run(() =>
            {
                foreach (var network in subnet.Subnet(newCidr))
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        lock (SubnetsResult)
                            SubnetsResult.Add(new IPNetworkInfo(network));
                    }));
                }
            });

            IsResultVisible = true;

            AddSubnetToHistory(Subnet);
            AddNewSubnetmaskOrCIDRToHistory(NewSubnetmaskOrCIDR);

            IsCalculationRunning = false;
        }

        private void AddSubnetToHistory(string subnet)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.Clear();
            OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.Add(x));
        }

        private void AddNewSubnetmaskOrCIDRToHistory(string newSubnetmaskOrCIDR)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory.ToList(), newSubnetmaskOrCIDR, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory.Clear();
            OnPropertyChanged(nameof(NewSubnetmaskOrCIDR)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskOrCIDRHistory.Add(x));
        }

        public void OnShutdown()
        {

        }
        #endregion
    }
}