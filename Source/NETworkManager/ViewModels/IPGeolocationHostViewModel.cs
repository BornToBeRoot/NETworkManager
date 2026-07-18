using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Profiles;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the IP geolocation host view.
/// </summary>
public class IPGeolocationHostViewModel : ProfileHostViewModelBase
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
    /// Initializes a new instance of the <see cref="IPGeolocationHostViewModel"/> class.
    /// </summary>
    public IPGeolocationHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.IPGeolocation);
        InterTabPartition = nameof(ApplicationName.IPGeolocation);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new IPGeolocationView(tabId), tabId)
        ];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.IPGeolocation;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.IPGeolocation_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.IPGeolocation_Host;

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
    /// Gets the command to query the selected profile.
    /// </summary>
    public ICommand QueryProfileCommand => new RelayCommand(_ => QueryProfileAction(), QueryProfile_CanExecute);

    /// <summary>
    /// Checks if the query profile command can be executed.
    /// </summary>
    private bool QueryProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    /// <summary>
    /// Action to query the selected profile.
    /// </summary>
    private void QueryProfileAction()
    {
        AddTab(SelectedProfile.IPGeolocation_Host);
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
    /// Adds a new tab for the specified domain.
    /// </summary>
    /// <param name="domain">The domain to query.</param>
    private void AddTab(string domain = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(domain ?? Strings.NewTab,
            new IPGeolocationView(tabId, domain), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    #endregion
}
