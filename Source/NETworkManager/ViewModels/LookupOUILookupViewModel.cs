using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
    /// <param name="instance">The dialog coordinator instance.</param>
    public LookupOUILookupViewModel(IDialogCoordinator instance)
    {
        _dialogCoordinator = instance;

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
    /// The dialog coordinator.
    /// </summary>
    private readonly IDialogCoordinator _dialogCoordinator;

    /// <summary>
    /// Backing field for <see cref="Search"/>.
    /// </summary>
    private string _search;

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="HasError"/>.
    /// </summary>
    private bool _hasError;

    /// <summary>
    /// Gets or sets a value indicating whether there is a validation error.
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        set
        {
            if (value == _hasError)
                return;

            _hasError = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the search history view.
    /// </summary>
    public ICollectionView SearchHistoryView { get; }

    /// <summary>
    /// Backing field for <see cref="IsRunning"/>.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the lookup is running.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="Results"/>.
    /// </summary>
    private ObservableCollection<OUIInfo> _results = new();

    /// <summary>
    /// Gets or sets the search results.
    /// </summary>
    public ObservableCollection<OUIInfo> Results
    {
        get => _results;
        set
        {
            if (value != null && value == _results)
                return;

            _results = value;
        }
    }

    /// <summary>
    /// Gets the results view.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedResult"/>.
    /// </summary>
    private OUIInfo _selectedResult;

    /// <summary>
    /// Gets or sets the selected result.
    /// </summary>
    public OUIInfo SelectedResult
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

    /// <summary>
    /// Backing field for <see cref="SelectedResults"/>.
    /// </summary>
    private IList _selectedResults = new ArrayList();

    /// <summary>
    /// Gets or sets the list of selected results.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="NothingFound"/>.
    /// </summary>
    private bool _nothingFound;

    /// <summary>
    /// Gets or sets a value indicating whether no results were found.
    /// </summary>
    public bool NothingFound
    {
        get => _nothingFound;
        set
        {
            if (value == _nothingFound)
                return;

            _nothingFound = value;
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

                var settings = AppearanceManager.MetroDialog;
                settings.AffirmativeButtonText = Strings.OK;

                await _dialogCoordinator.ShowMessageAsync(this, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, MessageDialogStyle.Affirmative, settings);
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

        return (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    #endregion
}