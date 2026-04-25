using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Lookup;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the OUI lookup view.
/// </summary>
public class LookupOUILookupViewModel : ViewModelBase
{
    #region Constructor, Load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="LookupOUILookupViewModel"/> class.
    /// </summary>
    public LookupOUILookupViewModel()
    {
        // Search history
        SearchHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_OUI_SearchHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.SortDescriptions.Add(new SortDescription(nameof(OUIInfo.MACAddress), ListSortDirection.Ascending));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the search term to the history.
    /// </summary>
    /// <param name="macAddressOrVendor">The MAC address or vendor.</param>
    private void AddSearchToHistory(string macAddressOrVendor)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.Lookup_OUI_SearchHistory.ToList(),
            macAddressOrVendor, SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.Lookup_OUI_SearchHistory.Clear();
        OnPropertyChanged(
            nameof(Search)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.Lookup_OUI_SearchHistory.Add(x));
    }

    #endregion

    #region Variables

    /// <summary>
    /// The logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(LookupOUILookupViewModel));

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    public string Search
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

    /// <summary>
    /// Gets or sets a value indicating whether there is a validation error.
    /// </summary>
    public bool HasError
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

    /// <summary>
    /// Gets the search history view.
    /// </summary>
    public ICollectionView SearchHistoryView { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the lookup is running.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the search results.
    /// </summary>
    public ObservableCollection<OUIInfo> Results
    {
        get;
        set
        {
            if (value != null && value == field)
                return;

            field = value;
        }
    } = new();

    /// <summary>
    /// Gets the results view.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Gets or sets the selected result.
    /// </summary>
    public OUIInfo SelectedResult
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

    /// <summary>
    /// Gets or sets the list of selected results.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether no results were found.
    /// </summary>
    public bool NothingFound
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

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to perform the OUI lookup.
    /// </summary>
    public ICommand OUILookupCommand => new RelayCommand(_ => OUILookupAction(), OUILookup_CanExecute);

    /// <summary>
    /// Checks if the OUI lookup command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool OUILookup_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen &&
               !HasError;
    }

    /// <summary>
    /// Performs the OUI lookup.
    /// </summary>
    private async void OUILookupAction()
    {
        IsRunning = true;

        Results.Clear();

        var macAddresses = new HashSet<string>();
        var vendors = new HashSet<string>();

        // Parse the input
        foreach (var search in Search.Split(';'))
        {
            var searchTrim = search.Trim();

            if (Regex.IsMatch(searchTrim, RegexHelper.MACAddressRegex) ||
                Regex.IsMatch(searchTrim, RegexHelper.MACAddressFirst3BytesRegex))
                macAddresses.Add(searchTrim);
            else
                vendors.Add(searchTrim);
        }

        // Temporary collection to avoid duplicate entries
        var results = new HashSet<OUIInfo>();

        // Get OUI information's by MAC-Address
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator (Doesn't work with async/await)
        foreach (var macAddress in macAddresses)
            foreach (var info in await OUILookup.LookupByMacAddressAsync(macAddress))
                results.Add(info);

        // Get OUI information's by Vendor
        foreach (var info in await OUILookup.SearchByVendorsAsync(vendors)) results.Add(info);

        // Add the results to the collection
        foreach (var result in results)
            Results.Add(result);

        // Show a message if no vendor was found
        NothingFound = Results.Count == 0;

        // Add the MAC-Address or Vendor to the history
        AddSearchToHistory(Search);

        IsRunning = false;
    }

    /// <summary>
    /// Gets the command to export the results.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(_ => ExportAction().ConfigureAwait(false));

    /// <summary>
    /// Exports the results.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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
                        : new ObservableCollection<OUIInfo>(SelectedResults.Cast<OUIInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.Lookup_OUI_ExportFileType = instance.FileType;
            SettingsManager.Current.Lookup_OUI_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
        [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.Lookup_OUI_ExportFileType, SettingsManager.Current.Lookup_OUI_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion
}