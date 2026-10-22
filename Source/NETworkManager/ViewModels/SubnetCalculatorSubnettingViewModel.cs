using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

public class SubnetCalculatorSubnettingViewModel : ViewModelBase
{
    #region Constructor, load settings

    public SubnetCalculatorSubnettingViewModel()
    {
        // Set collection view
        SubnetHistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory);
        NewSubnetmaskHistoryView =
            CollectionViewSource.GetDefaultView(
                SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
    }

    #endregion

    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(SubnetCalculatorSubnettingViewModel));

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

    public string NewSubnetmask
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

    public ICollectionView NewSubnetmaskHistoryView { get; }

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

    public ObservableCollection<IPNetworkInfo> Results
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ICollectionView ResultsView { get; }

    public IPNetworkInfo SelectedResult
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


    public IList SelectedResults
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

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
        _ = Calculate();
    }

    public ICommand ExportCommand => new RelayCommand(parameter => { _ = ExportAction(); });

    private Task ExportAction()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<IPNetworkInfo>(SelectedResults.Cast<IPNetworkInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.SubnetCalculator_Subnetting_ExportFileType = instance.FileType;
            SettingsManager.Current.SubnetCalculator_Subnetting_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.SubnetCalculator_Subnetting_ExportFileType,
        SettingsManager.Current.SubnetCalculator_Subnetting_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Splits the subnet into subnets (e.g. from the run command).
    /// </summary>
    /// <param name="subnet">Subnet (e.g. 10.0.0.0/16).</param>
    /// <param name="newSubnetmask">New subnet mask or CIDR (e.g. /24).</param>
    /// <param name="start">Calculates the subnets. If false, the values are only filled in.</param>
    public Task Calculate(string subnet, string newSubnetmask, bool start = true)
    {
        // Like the calculate button, not while a calculation is running (the results would be mixed).
        if (IsRunning)
            return Task.CompletedTask;

        Subnet = subnet;
        NewSubnetmask = newSubnetmask;

        return start ? Calculate() : Task.CompletedTask;
    }

    private async Task Calculate()
    {
        IsRunning = true;

        Results.Clear();

        var subnet = Subnet.Trim();
        var newSubnetmaskOrCidr = NewSubnetmask.Trim();

        var ipNetwork = IPNetwork2.Parse(Subnet.Trim());

        // Subnet mask (e.g. 255.255.255.0) or CIDR (e.g. /24)
        var newCidr = Subnetmask.ConvertSubnetmaskOrCidrToCidr(newSubnetmaskOrCidr);

        var (minCidr, baseCidr) = Subnetmask.GetSubnettingCidrRange(ipNetwork);

        // The new subnet mask must split the subnet (e.g. /24 into /26), otherwise the subnetting fails.
        if (newCidr < minCidr || newCidr > baseCidr)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                string.Format(Strings.EnterSubnetmaskOrCIDRBetweenXAndY, minCidr, baseCidr),
                ChildWindowIcon.Error);

            IsRunning = false;

            return;
        }

        // Ask the user if there is a large calculation...

        if (65535 < Math.Pow(2, baseCidr - ipNetwork.Cidr) / Math.Pow(2, baseCidr - newCidr))
        {
            if (!await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow, Strings.AreYouSure, Strings.TheProcessCanTakeUpSomeTimeAndResources))
            {
                IsRunning = false;

                return;
            }
        }

        // This still slows the application / freezes the ui... there are to many updates to the ui thread...
        await Task.Run(() =>
        {
            foreach (var network in ipNetwork.Subnet((byte)newCidr))
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(delegate { Results.Add(new IPNetworkInfo(network)); }));
        });

        IsResultVisible = true;

        ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory, subnet,
            SettingsManager.Current.General_HistoryListEntries);
        ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskHistory, newSubnetmaskOrCidr,
            SettingsManager.Current.General_HistoryListEntries);

        IsRunning = false;
    }

    public void OnShutdown()
    {
    }

    #endregion
}