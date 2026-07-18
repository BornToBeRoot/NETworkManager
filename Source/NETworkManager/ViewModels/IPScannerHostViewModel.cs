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
/// View model for the IP scanner host view.
/// </summary>
public class IPScannerHostViewModel : ProfileHostViewModelBase
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
    /// Initializes a new instance of the <see cref="IPScannerHostViewModel"/> class.
    /// </summary>
    public IPScannerHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.IPScanner);
        InterTabPartition = nameof(ApplicationName.IPScanner);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new IPScannerView(tabId), tabId)
        ];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.IPScanner;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.IPScanner_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.IPScanner_HostOrIPRange;

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
    /// Gets the command to scan the selected profile.
    /// </summary>
    public ICommand ScanProfileCommand => new RelayCommand(_ => ScanProfileAction(), ScanProfile_CanExecute);

    /// <summary>
    /// Checks if the scan profile command can be executed.
    /// </summary>
    private bool ScanProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    /// <summary>
    /// Action to scan the selected profile.
    /// </summary>
    private void ScanProfileAction()
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
    /// Adds a new tab for the specified host or IP range.
    /// </summary>
    /// <param name="hostOrIPRange">The host or IP range to scan.</param>
    public void AddTab(string hostOrIPRange = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(Strings.NewTab, new IPScannerView(tabId, hostOrIPRange),
            tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    /// <summary>
    /// Adds a new tab for the specified profile.
    /// </summary>
    /// <param name="profile">The profile to scan.</param>
    private void AddTab(ProfileInfo profile)
    {
        AddTab(profile.IPScanner_HostOrIPRange);
    }

    #endregion
}
