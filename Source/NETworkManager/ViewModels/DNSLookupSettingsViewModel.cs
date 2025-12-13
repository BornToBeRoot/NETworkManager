using DnsClient;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the DNS lookup settings.
/// </summary>
public class DNSLookupSettingsViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Default values for the profile dialog.
    /// </summary>
    private readonly ServerConnectionInfo _profileDialogDefaultValues = new("10.0.0.1", 53, TransportProtocol.Udp);

    /// <summary>
    /// Gets the collection view of DNS servers.
    /// </summary>
    public ICollectionView DNSServers { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedDNSServer"/>.
    /// </summary>
    private DNSServerConnectionInfoProfile _selectedDNSServer = new();

    /// <summary>
    /// Gets or sets the selected DNS server.
    /// </summary>
    public DNSServerConnectionInfoProfile SelectedDNSServer
    {
        get => _selectedDNSServer;
        set
        {
            if (value == _selectedDNSServer)
                return;

            _selectedDNSServer = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of server info profile names.
    /// </summary>
    private List<string> ServerInfoProfileNames =>
    [
        .. SettingsManager.Current.DNSLookup_DNSServers
            .Where(x => !x.UseWindowsDNSServer).Select(x => x.Name)
    ];

    /// <summary>
    /// Backing field for <see cref="AddDNSSuffix"/>.
    /// </summary>
    private bool _addDNSSuffix;

    /// <summary>
    /// Gets or sets a value indicating whether to add DNS suffix.
    /// </summary>
    public bool AddDNSSuffix
    {
        get => _addDNSSuffix;
        set
        {
            if (value == _addDNSSuffix)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_AddDNSSuffix = value;

            _addDNSSuffix = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="UseCustomDNSSuffix"/>.
    /// </summary>
    private bool _useCustomDNSSuffix;

    /// <summary>
    /// Gets or sets a value indicating whether to use a custom DNS suffix.
    /// </summary>
    public bool UseCustomDNSSuffix
    {
        get => _useCustomDNSSuffix;
        set
        {
            if (value == _useCustomDNSSuffix)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseCustomDNSSuffix = value;

            _useCustomDNSSuffix = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="CustomDNSSuffix"/>.
    /// </summary>
    private string _customDNSSuffix;

    /// <summary>
    /// Gets or sets the custom DNS suffix.
    /// </summary>
    public string CustomDNSSuffix
    {
        get => _customDNSSuffix;
        set
        {
            if (value == _customDNSSuffix)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_CustomDNSSuffix = value;

            _customDNSSuffix = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Recursion"/>.
    /// </summary>
    private bool _recursion;

    /// <summary>
    /// Gets or sets a value indicating whether recursion is enabled.
    /// </summary>
    public bool Recursion
    {
        get => _recursion;
        set
        {
            if (value == _recursion)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Recursion = value;

            _recursion = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="UseCache"/>.
    /// </summary>
    private bool _useCache;

    /// <summary>
    /// Gets or sets a value indicating whether to use cache.
    /// </summary>
    public bool UseCache
    {
        get => _useCache;
        set
        {
            if (value == _useCache)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseCache = value;

            _useCache = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of available query classes.
    /// </summary>
    public List<QueryClass> QueryClasses { get; private set; }

    /// <summary>
    /// Backing field for <see cref="QueryClass"/>.
    /// </summary>
    private QueryClass _queryClass;

    /// <summary>
    /// Gets or sets the selected query class.
    /// </summary>
    public QueryClass QueryClass
    {
        get => _queryClass;
        set
        {
            if (value == _queryClass)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_QueryClass = value;

            _queryClass = value;
            OnPropertyChanged();
        }
    }

    /*
     * Disabled until more query types are implemented.


    private bool _showOnlyMostCommonQueryTypes;

    public bool ShowOnlyMostCommonQueryTypes
    {
        get => _showOnlyMostCommonQueryTypes;
        set
        {
            if (value == _showOnlyMostCommonQueryTypes)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes = value;

            _showOnlyMostCommonQueryTypes = value;
            OnPropertyChanged();
        }
    }
    */

    /// <summary>
    /// Backing field for <see cref="UseTCPOnly"/>.
    /// </summary>
    private bool _useTCPOnly;

    /// <summary>
    /// Gets or sets a value indicating whether to use TCP only.
    /// </summary>
    public bool UseTCPOnly
    {
        get => _useTCPOnly;
        set
        {
            if (value == _useTCPOnly)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseTCPOnly = value;

            _useTCPOnly = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Retries"/>.
    /// </summary>
    private int _retries;

    /// <summary>
    /// Gets or sets the number of retries.
    /// </summary>
    public int Retries
    {
        get => _retries;
        set
        {
            if (value == _retries)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Retries = value;

            _retries = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Timeout"/>.
    /// </summary>
    private int _timeout;

    /// <summary>
    /// Gets or sets the timeout in milliseconds.
    /// </summary>
    public int Timeout
    {
        get => _timeout;
        set
        {
            if (value == _timeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Timeout = value;

            _timeout = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="DNSLookupSettingsViewModel"/> class.
    /// </summary>
    public DNSLookupSettingsViewModel()
    {
        _isLoading = true;

        DNSServers = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_DNSServers);
        DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerConnectionInfoProfile.Name),
            ListSortDirection.Ascending));
        DNSServers.Filter = o =>
        {
            if (o is not DNSServerConnectionInfoProfile info)
                return false;

            return !info.UseWindowsDNSServer;
        };

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix;
        UseCustomDNSSuffix = SettingsManager.Current.DNSLookup_UseCustomDNSSuffix;
        CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix;
        Recursion = SettingsManager.Current.DNSLookup_Recursion;
        UseCache = SettingsManager.Current.DNSLookup_UseCache;
        QueryClasses = Enum.GetValues(typeof(QueryClass)).Cast<QueryClass>().OrderBy(x => x.ToString()).ToList();
        QueryClass = QueryClasses.First(x => x == SettingsManager.Current.DNSLookup_QueryClass);
        //ShowOnlyMostCommonQueryTypes = SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes;
        UseTCPOnly = SettingsManager.Current.DNSLookup_UseTCPOnly;
        Retries = SettingsManager.Current.DNSLookup_Retries;
        Timeout = SettingsManager.Current.DNSLookup_Timeout;
    }

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to add a DNS server.
    /// </summary>
    public ICommand AddDNSServerCommand => new RelayCommand(_ => AddDNSServerAction());

    /// <summary>
    /// Action to add a DNS server.
    /// </summary>
    private void AddDNSServerAction()
    {
        AddDNSServer().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to edit a DNS server.
    /// </summary>
    public ICommand EditDNSServerCommand => new RelayCommand(_ => EditDNSServerAction());

    /// <summary>
    /// Action to edit a DNS server.
    /// </summary>
    private void EditDNSServerAction()
    {
        EditDNSServer().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the command to delete a DNS server.
    /// </summary>
    public ICommand DeleteDNSServerCommand => new RelayCommand(_ => DeleteDNSServerAction());

    /// <summary>
    /// Action to delete a DNS server.
    /// </summary>
    private void DeleteDNSServerAction()
    {
        DeleteDNSServer().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds a new DNS server.
    /// </summary>
    private async Task AddDNSServer()
    {
        var childWindow = new ServerConnectionInfoProfileChildWindow();

        var childWindowViewModel = new ServerConnectionInfoProfileViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.DNSLookup_DNSServers.Add(
                new DNSServerConnectionInfoProfile(instance.Name, [.. instance.Servers]));
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            (ServerInfoProfileNames, false, false),
            _profileDialogDefaultValues);

        childWindow.Title = Strings.AddDNSServer;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Edits the selected DNS server.
    /// </summary>
    public async Task EditDNSServer()
    {
        var childWindow = new ServerConnectionInfoProfileChildWindow();

        var childWindowViewModel = new ServerConnectionInfoProfileViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
            SettingsManager.Current.DNSLookup_DNSServers.Add(
                    new DNSServerConnectionInfoProfile(instance.Name, [.. instance.Servers]));
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            (ServerInfoProfileNames, true, false),
            _profileDialogDefaultValues, SelectedDNSServer);

        childWindow.Title = Strings.EditDNSServer;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Deletes the selected DNS server.
    /// </summary>
    private async Task DeleteDNSServer()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.DeleteDNSServer,
            Strings.DeleteDNSServerMessage,
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
    }

    #endregion
}