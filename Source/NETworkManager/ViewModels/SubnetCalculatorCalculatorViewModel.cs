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

    /// <summary>
    ///     Calculates the subnet (e.g. from the run command).
    /// </summary>
    /// <param name="subnet">Subnet (e.g. 10.0.0.0/24).</param>
    /// <param name="start">Calculates the subnet. If false, the subnet is only filled in.</param>
    public void Calculate(string subnet, bool start = true)
    {
        Subnet = subnet;

        if (start)
            Calculate();
    }

    private void Calculate()
    {
        IsRunning = true;

        var subnet = Subnet.Trim();

        Result = new IPNetworkInfo(IPNetwork2.Parse(subnet));

        IsResultVisible = true;

        ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Calculator_SubnetHistory, subnet,
            SettingsManager.Current.General_HistoryListEntries);

        IsRunning = false;
    }

    #endregion
}