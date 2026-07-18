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
/// The view model for the Port Scanner host, managing tabs and profiles.
/// </summary>
public class PortScannerHostViewModel : ProfileHostViewModelBase
{
    #region Variables

    /// <summary>
    /// Gets the InterTabClient for Dragablz.
    /// </summary>
    public IInterTabClient InterTabClient { get; }

    /// <summary>
    /// Gets or sets the InterTab partition identifier.
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
    /// Initializes a new instance of the <see cref="PortScannerHostViewModel"/> class.
    /// </summary>
    public PortScannerHostViewModel()
    {
        InterTabClient = new DragablzInterTabClient(ApplicationName.PortScanner);
        InterTabPartition = nameof(ApplicationName.PortScanner);

        var tabId = Guid.NewGuid();

        TabItems =
        [
            new DragablzTabItem(Strings.NewTab, new PortScannerView(tabId), tabId)
        ];

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.PortScanner;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.PortScanner_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.PortScanner_Host;

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to add a new tab.
    /// </summary>
    public ICommand AddTabCommand => new RelayCommand(_ => AddTabAction());

    private void AddTabAction()
    {
        AddTab();
    }

    /// <summary>
    /// Gets the command to scan the selected profile.
    /// </summary>
    public ICommand ScanProfileCommand => new RelayCommand(_ => ScanProfileAction(), ScanProfile_CanExecute);

    private bool ScanProfile_CanExecute(object obj)
    {
        return !IsSearching && SelectedProfile != null;
    }

    private void ScanProfileAction()
    {
        AddTab(SelectedProfile);
    }

    /// <summary>
    /// Gets the callback for closing a tab item.
    /// </summary>
    public ItemActionCallback CloseItemCommand => CloseItemAction;

    private static void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
    {
        ((args.DragablzItem.Content as DragablzTabItem)?.View as IDragablzTabItem)?.CloseTab();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds a new tab with the specified host and ports.
    /// </summary>
    /// <param name="host">The host to scan.</param>
    /// <param name="ports">The ports to scan.</param>
    public void AddTab(string host = null, string ports = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(string.IsNullOrEmpty(host) ? Strings.NewTab : host,
            new PortScannerView(tabId, host, ports), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    private void AddTab(ProfileInfo profile)
    {
        AddTab(profile.PortScanner_Host, profile.PortScanner_Ports);
    }

    #endregion
}
