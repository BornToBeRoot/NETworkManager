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

public class SubnetCalculatorWideSubnetViewModel : ViewModelBase
{
    #region Constructor, load settings

    public SubnetCalculatorWideSubnetViewModel()
    {
        // Set collection view
        Subnet1HistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1);
        Subnet2HistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet2);
    }

    #endregion

    #region Variables

    public string Subnet1
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

    public ICollectionView Subnet1HistoryView { get; }

    public string Subnet2
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

    public ICollectionView Subnet2HistoryView { get; }

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

    #region ICommands & Actions

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

        var subnet1 = Subnet1.Trim();
        var subnet2 = Subnet2.Trim();

        var ipNetwork1 = IPNetwork2.Parse(subnet1);
        var ipNetwork2 = IPNetwork2.Parse(subnet2);

        Result = new IPNetworkInfo(IPNetwork2.WideSubnet([ipNetwork1, ipNetwork2]));

        IsResultVisible = true;

        AddSubnet1ToHistory(subnet1);
        AddSubnet2ToHistory(subnet2);

        IsRunning = false;
    }

    private void AddSubnet1ToHistory(string subnet)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1.ToList(), subnet,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1.Clear();
        OnPropertyChanged(nameof(Subnet1)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet1.Add(x));
    }

    private void AddSubnet2ToHistory(string subnet)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_WideSubnet_Subnet2.ToList(), subnet,
            SettingsManager.Current.General_HistoryListEntries);

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