using DnsClient;
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
    /// Gets or sets the selected DNS server.
    /// </summary>
    public DNSServerConnectionInfoProfile SelectedDNSServer
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
    /// Gets the list of server info profile names.
    /// </summary>
    private List<string> ServerInfoProfileNames =>
    [
        .. SettingsManager.Current.DNSLookup_DNSServers
            .Where(x => !x.UseWindowsDNSServer).Select(x => x.Name)
    ];

    /// <summary>
    /// Gets or sets a value indicating whether to add DNS suffix.
    /// </summary>
    public bool AddDNSSuffix
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_AddDNSSuffix = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use a custom DNS suffix.
    /// </summary>
    public bool UseCustomDNSSuffix
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseCustomDNSSuffix = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the custom DNS suffix.
    /// </summary>
    public string CustomDNSSuffix
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_CustomDNSSuffix = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether recursion is enabled.
    /// </summary>
    public bool Recursion
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Recursion = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use cache.
    /// </summary>
    public bool UseCache
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseCache = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of available query classes.
    /// </summary>
    public List<QueryClass> QueryClasses { get; private set; }

    /// <summary>
    /// Gets or sets the selected query class.
    /// </summary>
    public QueryClass QueryClass
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_QueryClass = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use TCP only.
    /// </summary>
    public bool UseTCPOnly
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseTCPOnly = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the number of retries.
    /// </summary>
    public int Retries
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Retries = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the timeout in milliseconds.
    /// </summary>
    public int Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Timeout = value;

            field = value;
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

        SelectedDNSServer = DNSServers.Cast<DNSServerConnectionInfoProfile>().FirstOrDefault(x => !x.UseWindowsDNSServer);

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
        QueryClasses = [.. Enum.GetValues<QueryClass>().OrderBy(x => x.ToString())];
        QueryClass = QueryClasses.First(x => x == SettingsManager.Current.DNSLookup_QueryClass);
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

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
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

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
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