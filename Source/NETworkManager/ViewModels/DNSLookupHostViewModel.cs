using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Profiles;
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

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.DNSLookup_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.DNSLookup_Host;

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
        AddTab(SelectedProfile.DNSLookup_Host);
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
    public void AddTab(string host = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(host ?? Strings.NewTab, new DNSLookupView(tabId, host),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    #endregion
}
