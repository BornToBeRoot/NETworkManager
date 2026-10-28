using DnsClient;
using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the DNS lookup host view.
/// </summary>
public class DNSLookupHostViewModel : ProfileHostViewModelBase
{
    #region Variables

    /// <summary>
    /// Gets the client for inter-tab operations.
    /// </summary>
    public IInterTabClient InterTabClient { get; }

    /// <summary>
    /// Gets or sets the inter-tab partition key.
    /// </summary>
    public string InterTabPartition
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
    /// Gets the collection of tab items.
    /// </summary>
    public ObservableCollection<DragablzTabItem> TabItems { get; }

    /// <summary>
    /// Gets or sets the index of the selected tab.
    /// </summary>
    public int SelectedTabIndex
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

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DNSLookupHostViewModel"/> class.
    /// </summary>
    public DNSLookupHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.DNSLookup);
        InterTabPartition = nameof(ApplicationName.DNSLookup);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new DNSLookupView(tabId), tabId)
        ];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.DNSLookup;

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to add a new tab.
    /// </summary>
    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

    /// <summary>
    /// Action to add a new tab.
    /// </summary>
    private void AddTabAction()
    {
        AddTab();
    }

    /// <summary>
    /// Gets the command to lookup the selected profile.
    /// </summary>
    public ICommand LookupProfileCommand => new RelayCommand(_ => LookupProfileAction(), LookupProfile_CanExecute);

    /// <summary>
    /// Checks if the lookup profile command can be executed.
    /// </summary>
    private bool LookupProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    /// <summary>
    /// Action to lookup the selected profile.
    /// </summary>
    private void LookupProfileAction()
    {
        AddTab(SelectedProfile);
    }

    /// <summary>
    /// Gets the callback for closing a tab item.
    /// </summary>
    public ItemActionCallback CloseItemCommand => CloseItemAction;

    /// <summary>
    /// Action to close a tab item.
    /// </summary>
    private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds a new tab for the specified host.
    /// </summary>
    /// <param name="host">The host to lookup.</param>
    /// <param name="dnsServer">DNS server(s) to use. If null, the selected DNS server is used.</param>
    /// <param name="queryType">
    ///     Query type. If null, PTR is used for IP addresses (e.g. from the IP Scanner) and the selected query type for
    ///     hosts.
    /// </param>
    /// <param name="start">Starts it on the first load. If false, the input is only filled in.</param>
    public void AddTab(string host = null, string dnsServer = null, QueryType? queryType = null, bool start = true,
        string message = null)
    {
        if (queryType == null && IPAddressHelper.AreIPAddresses(host))
            queryType = QueryType.PTR;

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(host ?? Strings.NewTab,
            new DNSLookupView(tabId, host, dnsServer, queryType, start, message), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    /// <summary>
    /// Adds a new tab for the host of the specified profile.
    /// </summary>
    /// <param name="profile">The profile to lookup.</param>
    /// <param name="start">Starts it on the first load. If false, the input is only filled in.</param>
    public void AddTab(ProfileInfo profile, bool start = true)
    {
        // DNS server of DNS Lookup (last selected)
        if (!profile.DNSLookup_OverrideDNSServer || string.IsNullOrWhiteSpace(profile.DNSLookup_DNSServer))
        {
            AddTab(profile.DNSLookup_Host, start: start);
            return;
        }

        // DNS servers by name with their current name (also after a rename)
        if (!string.IsNullOrEmpty(profile.DNSLookup_DNSServerInput) &&
            DNSServer.TryDecodeReferences(profile.DNSLookup_DNSServerInput,
                SettingsManager.Current.DNSLookup_DNSServers, out var dnsServer))
        {
            AddTab(profile.DNSLookup_Host, dnsServer, start: start);
            return;
        }

        // No DNS server by name, or one can't be found (deleted, or a profile file shared from another computer - DNS
        // servers are saved in the settings): the servers saved with the profile, with a note if one wasn't found
        AddTab(profile.DNSLookup_Host, profile.DNSLookup_DNSServer, start: start,
            message: string.IsNullOrEmpty(profile.DNSLookup_DNSServerInput)
                ? null
                : Strings.DNSServerOfProfileNotFoundMessage);
    }

    #endregion
}
