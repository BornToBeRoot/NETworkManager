using NETworkManager.Settings;
using System.Windows.Input;
using NETworkManager.Utilities;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;

namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorWideSubnetViewModel : ViewModelBase
    {
        #region Variables
        private readonly IDialogCoordinator _dialogCoordinator;

        private string _subnet1;
        public string Subnet1
        {
            get => _subnet1;
            set
            {
                if (value == _subnet1)
                    return;

                _subnet1 = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView Subnet1HistoryView { get; }

        private string _subnet2;
        public string Subnet2
        {
            get => _subnet2;
            set
            {
                if (value == _subnet2)
                    return;

                _subnet2 = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView Subnet2HistoryView { get; }

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

        private IPNetworkInfo _result;
        public IPNetworkInfo Result
        {
            get => _result;
            set
            {
                if (value == _result)
                    return;

                _result = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public SubnetCalculatorWideSubnetViewModel(IDialogCoordinator instance)
        {
            _dialogCoordinator = instance;

            // Set collection view
            Subnet1HistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1);
            Subnet2HistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet2);
        }
        #endregion

        #region ICommands & Actions
        public ICommand CalculateCommand => new RelayCommand(p => CalculateAction(), Calculate_CanExecute);

        private bool Calculate_CanExecute(object paramter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

        private void CalculateAction()
        {
            Calculate();
        }
        #endregion

        #region Methods
        private void Calculate()
        {
            IsCalculationRunning = true;

            var subnet1 = IPNetwork.Parse(Subnet1);
            var subnet2 = IPNetwork.Parse(Subnet2);

            Result = new IPNetworkInfo(IPNetwork.WideSubnet(new[] { subnet1, subnet2 }));

            IsResultVisible = true;

            AddSubnet1ToHistory(Subnet1);
            AddSubnet2ToHistory(Subnet2);

            IsCalculationRunning = false;
        }

        private void AddSubnet1ToHistory(string subnet)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1.Clear();
            OnPropertyChanged(nameof(Subnet1)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1.Add(x));
        }

        private void AddSubnet2ToHistory(string subnet)
        {
            // Create the new list
            var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet2.ToList(), subnet, SettingsManager.Current.General_HistoryListEntries);

            // Clear the old items
            SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet2.Clear();
            OnPropertyChanged(nameof(Subnet2)); // Raise property changed again, after the collection has been cleared

            // Fill with the new items
            list.ForEach(x => SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet2.Add(x));
        }

        public void OnShutdown()
        {

        }
        #endregion
    }
}