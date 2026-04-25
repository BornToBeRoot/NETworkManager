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
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using IPAddress = System.Net.IPAddress;

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
        Calculate().ConfigureAwait(false);
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

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

    private async Task Calculate()
    {
        IsRunning = true;

        Results.Clear();

        var subnet = Subnet.Trim();
        var newSubnetmaskOrCidr = NewSubnetmask.Trim();

        var ipNetwork = IPNetwork2.Parse(Subnet.Trim());

        var newCidr =
            // Support subnetmask like 255.255.255.0
            RegexHelper.SubnetmaskRegex().IsMatch(newSubnetmaskOrCidr)
                ? Convert.ToByte(Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(newSubnetmaskOrCidr)))
                : Convert.ToByte(newSubnetmaskOrCidr.TrimStart('/'));

        // Ask the user if there is a large calculation...
        var baseCidr = ipNetwork.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;

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
            foreach (var network in ipNetwork.Subnet(newCidr))
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(delegate { Results.Add(new IPNetworkInfo(network)); }));
        });

        IsResultVisible = true;

        AddSubnetToHistory(subnet);
        AddNewSubnetmaskOrCidrToHistory(newSubnetmaskOrCidr);

        IsRunning = false;
    }

    private void AddSubnetToHistory(string subnet)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.ToList(), subnet,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.Clear();
        OnPropertyChanged(nameof(Subnet)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.SubnetCalculator_Subnetting_SubnetHistory.Add(x));
    }

    private void AddNewSubnetmaskOrCidrToHistory(string newSubnetmaskOrCidr)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskHistory.ToList(),
            newSubnetmaskOrCidr, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskHistory.Clear();
        OnPropertyChanged(nameof(NewSubnetmask)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskHistory.Add(x));
    }

    public void OnShutdown()
    {
    }

    #endregion
}