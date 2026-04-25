using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class SubnetCalculatorCalculatorViewModel : ViewModelBase
{
    #region Constructor, load settings

    public SubnetCalculatorCalculatorViewModel()
    {
        SubnetHistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory);
    }

    #endregion

    #region Variables

    public string Subnet
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView SubnetHistoryView { get; }

    public bool IsRunning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsResultVisible
    {
        get;
        set
        {
            if (value == field)
                return;


            field = value;
            OnPropertyChanged();
        }
    }

    public IPNetworkInfo Result
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommands

    public ICommand CalculateCommand => new RelayCommand(_ => CalculateAction(), Calculate_CanExecute);

    private bool Calculate_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void CalculateAction()
    {
        Calculate();
    }

    #endregion

    #region Methods

    private void Calculate()
    {
        IsRunning = true;

        var subnet = Subnet.Trim();

        Result = new IPNetworkInfo(IPNetwork2.Parse(subnet));

        IsResultVisible = true;

        AddSubnetToHistory(subnet);

        IsRunning = false;
    }

    private void AddSubnetToHistory(string subnet)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.ToList(), subnet,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.Clear();
        OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory.Add(x));
    }

    #endregion
}