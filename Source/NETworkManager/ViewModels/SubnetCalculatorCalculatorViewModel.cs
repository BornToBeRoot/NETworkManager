using NETworkManager.Settings;
using System.Net;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using NETworkManager.Utilities;
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

        private IPNetwork _result;
        public IPNetwork Result
        {
            get => _result; 
            set
            {
                if(value== _result) 
                    return;

                _result = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public SubnetCalculatorCalculatorViewModel()
        {
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

            Result = IPNetwork.Parse(Subnet);

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