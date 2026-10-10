using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.RDAP;
using NETworkManager.Profiles;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class RDAPHostViewModel : ProfileHostViewModelBase
{
    #region Variables

    public IInterTabClient InterTabClient { get; }

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

    public ObservableCollection<DragablzTabItem> TabItems { get; }

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

    public RDAPHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.RDAP);
        InterTabPartition = nameof(ApplicationName.RDAP);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new RDAPView(tabId), tabId)
        ];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.RDAP;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.RDAP_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.RDAP_Query;

    #endregion

    #region ICommand & Actions

    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

    private void AddTabAction()
    {
        AddTab();
    }

    public ICommand QueryProfileCommand => new RelayCommand(_ => QueryProfileAction(), QueryProfile_CanExecute);

    private bool QueryProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void QueryProfileAction()
    {
        AddTab(SelectedProfile.RDAP_Query, SelectedProfile.RDAP_QueryType);
    }

    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Adds a new tab and starts the query, if one is given.
    /// </summary>
    /// <param name="query">Query (e.g. domain, IP address, AS number).</param>
    /// <param name="queryType">
    ///     Type of the query. If null, the type is detected from the query (e.g. data redirected from another tool or a
    ///     run command), or the last selected type is used for an empty tab.
    /// </param>
    public void AddTab(string query = null, RDAPQueryType? queryType = null)
    {
        if (queryType == null && !string.IsNullOrWhiteSpace(query))
            queryType = RDAPQueryParser.DetectType(query);

        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(query ?? Strings.NewTab, new RDAPView(tabId, query, queryType),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    #endregion
}
