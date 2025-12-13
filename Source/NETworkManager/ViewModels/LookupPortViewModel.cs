using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
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
/// View model for the port lookup view.
/// </summary>
public class LookupPortLookupViewModel : ViewModelBase
{
    #region Constructor, Load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="LookupPortLookupViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    public LookupPortLookupViewModel(IDialogCoordinator instance)
    {
        _dialogCoordinator = instance;

        SearchHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.Lookup_Port_SearchHistory);
        ResultsView = CollectionViewSource.GetDefaultView(Results);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the search term to the history.
    /// </summary>
    /// <param name="portOrService">The port or service.</param>
    private void AddSearchToHistory(string portOrService)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.Lookup_Port_SearchHistory.ToList(), portOrService,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.Lookup_Port_SearchHistory.Clear();
        OnPropertyChanged(nameof(Search)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.Lookup_Port_SearchHistory.Add(x));
    }

    #endregion

    #region Variables

    /// <summary>
    /// The logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(LookupPortLookupViewModel));

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
    private ObservableCollection<PortLookupInfo> _results = new();

    /// <summary>
    /// Gets or sets the search results.
    /// </summary>
    public ObservableCollection<PortLookupInfo> Results
    {
        get => _results;
        set
        {
            if (value != null && value == _results)
                return;

            _results = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the results view.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedResult"/>.
    /// </summary>
    private PortLookupInfo _selectedResult;

    /// <summary>
    /// Gets or sets the selected result.
    /// </summary>
    public PortLookupInfo SelectedResult
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
    /// Gets the command to perform the port lookup.
    /// </summary>
    public ICommand PortLookupCommand =>
        new RelayCommand(_ => PortLookupAction().ConfigureAwait(false), PortLookup_CanExecute);

    /// <summary>
    /// Checks if the port lookup command can be executed.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> if the command can be executed; otherwise, <c>false</c>.</returns>
    private bool PortLookup_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen && !HasError;
    }

    /// <summary>
    /// Performs the port lookup.
    /// </summary>
    private async Task PortLookupAction()
    {
        IsRunning = true;

        Results.Clear();

        var ports = new HashSet<int>();
        var portsAndProtocols = new HashSet<Tuple<int, string>>();
        var services = new HashSet<string>();

        foreach (var search in Search.Split(';'))
        {
            var searchTrim = search.Trim();

            // Check if the search is a port number like 22
            if (Regex.IsMatch(searchTrim, "^[0-9]{1,5}$"))
                if (int.TryParse(searchTrim, out var port))
                    if (port is > 0 and < 65536)
                    {
                        ports.Add(port);
                        continue;
                    }

            // Check if the search is a port number with protocol like 22/tcp
            if (Regex.IsMatch(searchTrim, "^[0-9]{1,5}/(tcp|udp|sctp)$"))
            {
                var portAndProtocol = searchTrim.Split('/');

                if (int.TryParse(portAndProtocol[0], out var port))
                    if (port is > 0 and < 65536)
                    {
                        portsAndProtocols.Add(new Tuple<int, string>(port, portAndProtocol[1]));
                        continue;
                    }
            }

            // Check if the search is a port range like 1-100
            if (Regex.IsMatch(searchTrim, "^[0-9]{1,5}-[0-9]{1,5}$"))
            {
                var portRange = searchTrim.Split('-');

                if (int.TryParse(portRange[0], out var startPort) && int.TryParse(portRange[1], out var endPort))
                    if (startPort is > 0 and < 65536 && endPort is > 0 and < 65536 && startPort < endPort)
                    {
                        for (var i = startPort; i < endPort + 1; i++)
                            ports.Add(i);

                        continue;
                    }
            }

            // Check if the search is a port range with protocol like 1-100/tcp
            if (Regex.IsMatch(searchTrim, "^[0-9]{1,5}-[0-9]{1,5}/(tcp|udp|sctp)$"))
            {
                var portRangeAndProtocol = searchTrim.Split('/');

                var portRange = portRangeAndProtocol[0].Split('-');

                if (int.TryParse(portRange[0], out var startPort) && int.TryParse(portRange[1], out var endPort))
                    if (startPort is > 0 and < 65536 && endPort is > 0 and < 65536 && startPort < endPort)
                    {
                        for (var i = startPort; i < endPort + 1; i++)
                            portsAndProtocols.Add(new Tuple<int, string>(i, portRangeAndProtocol[1]));

                        continue;
                    }
            }

            // Assume that everything else is a service like ssh
            services.Add(searchTrim);
        }

        // Temporary collection to avoid duplicate entries
        var results = new HashSet<PortLookupInfo>();

        // Get Port information's by port number
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator (Doesn't work with async/await)
        foreach (var port in ports)
            foreach (var info in await PortLookup.LookupByPortAsync(port))
                results.Add(info);

        // Get Port information's by port number and protocol
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator (Doesn't work with async/await)
        foreach (var portAndProtocol in portsAndProtocols)
            results.Add(
                await PortLookup.LookupByPortAndProtocolAsync(
                    portAndProtocol.Item1,
                    (TransportProtocol)Enum.Parse(typeof(TransportProtocol), portAndProtocol.Item2, true))
            );

        // Get Port information's by service
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator (Doesn't work with async/await)
        foreach (var service in services)
            foreach (var info in await PortLookup.SearchByServiceAsync(service))
                results.Add(info);

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
                        : new ObservableCollection<PortLookupInfo>(SelectedResults.Cast<PortLookupInfo>()
                            .ToArray()));
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

            SettingsManager.Current.Lookup_Port_ExportFileType = instance.FileType;
            SettingsManager.Current.Lookup_Port_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.Lookup_Port_ExportFileType,
        SettingsManager.Current.Lookup_Port_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    #endregion
}