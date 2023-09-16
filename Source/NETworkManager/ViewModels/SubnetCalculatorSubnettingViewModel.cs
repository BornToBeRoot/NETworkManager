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

namespace NETworkManager.ViewModels;

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

    private string _newSubnetmask;
    public string NewSubnetmask
    {
        get => _newSubnetmask;
        set
        {
            if (value == _newSubnetmask)
                return;

            _newSubnetmask = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView NewSubnetmaskHistoryView { get; }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
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

    private ObservableCollection<IPNetworkInfo> _results = new();
    public ObservableCollection<IPNetworkInfo> Results
    {
        get => _results;
        set
        {
            if (value == _results)
                return;

            _results = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView ResultsView { get; }

    private IPNetworkInfo _selectedResult;
    public IPNetworkInfo SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (value == _selectedResult)
                return;

            _selectedResult = value;
            OnPropertyChanged();
        }
    }


    private IList _selectedResults = new ArrayList();
    public IList SelectedResults
    {
        get => _selectedResults;
        set
        {
            if (Equals(value, _selectedResults))
                return;

            _selectedResults = value;
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
        NewSubnetmaskHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
    }
    #endregion

    #region ICommands & Actions
    public ICommand CalculateCommand => new RelayCommand(_ => CalculateAction(), Calculate_CanExecute);

    private bool Calculate_CanExecute(object parameter) => Application.Current.MainWindow != null && !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen;

    private void CalculateAction()
    {
        Calculate().ConfigureAwait(false);
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    private async Task ExportAction()
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
                ExportManager.Export(instance.FilePath, instance.FileType, instance.ExportAll ? Results : new ObservableCollection<IPNetworkInfo>(SelectedResults.Cast<IPNetworkInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Localization.Resources.Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.Error, Localization.Resources.Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine + Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
            }

            SettingsManager.Current.SubnetCalculator_Subnetting_ExportFileType = instance.FileType;
            SettingsManager.Current.SubnetCalculator_Subnetting_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);
        }, new[]
        {
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        }, true, SettingsManager.Current.SubnetCalculator_Subnetting_ExportFileType, SettingsManager.Current.SubnetCalculator_Subnetting_ExportFilePath);

        customDialog.Content = new ExportDialog
        {
            DataContext = exportViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }
    #endregion

    #region Methods
    private async Task Calculate()
    {            
        IsRunning = true;

        Results.Clear();

        var subnet = Subnet.Trim();
        var newSubnetmaskOrCidr = NewSubnetmask.Trim();                

        var ipNetwork = IPNetwork.Parse(Subnet.Trim());

        var newCidr =
            // Support subnetmask like 255.255.255.0
            Regex.IsMatch(newSubnetmaskOrCidr, RegexHelper.SubnetmaskRegex) ?
            Convert.ToByte(Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(newSubnetmaskOrCidr))) :
            Convert.ToByte(newSubnetmaskOrCidr.TrimStart('/'));

        // Ask the user if there is a large calculation...
        var baseCidr = ipNetwork.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;

        if (65535 < Math.Pow(2, baseCidr - ipNetwork.Cidr) / Math.Pow(2, (baseCidr - newCidr)))
        {
            var settings = AppearanceManager.MetroDialog;

            settings.AffirmativeButtonText = Localization.Resources.Strings.Continue;
            settings.NegativeButtonText = Localization.Resources.Strings.Cancel;

            settings.DefaultButtonFocus = MessageDialogResult.Affirmative;

            if (await _dialogCoordinator.ShowMessageAsync(this, Localization.Resources.Strings.AreYouSure, Localization.Resources.Strings.TheProcessCanTakeUpSomeTimeAndResources, MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
            {
                IsRunning = false;

                return;
            }
        }

        // This still slows the application / freezes the ui... there are to many updates to the ui thread...
        await Task.Run(() =>
        {
            foreach (var network in ipNetwork.Subnet(newCidr))
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {                        
                    Results.Add(new IPNetworkInfo(network));
                }));
            }
        });

        IsResultVisible = true;

        AddSubnetToHistory(subnet);
        AddNewSubnetmaskOrCidrToHistory(newSubnetmaskOrCidr);

        IsRunning = false;
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

    private void AddNewSubnetmaskOrCidrToHistory(string newSubnetmaskOrCidr)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SubnetCalculator_Subnetting_NewSubnetmaskHistory.ToList(), newSubnetmaskOrCidr, SettingsManager.Current.General_HistoryListEntries);

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