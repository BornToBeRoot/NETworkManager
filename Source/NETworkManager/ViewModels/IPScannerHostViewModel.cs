using Dragablz;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
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
    /// Gets the command to scan the selected profile with other Windows credentials (like Connect as... of Remote
    /// Desktop).
    /// </summary>
    public ICommand ScanProfileAsCommand => new RelayCommand(_ => ScanProfileAsAction(), ScanProfile_CanExecute);

    private void ScanProfileAsAction()
    {
        _ = ScanProfileAs();
    }

    /// <summary>
    /// Asks for the Windows credentials (the ones of the profile / group as default) and scans the selected profile with
    /// them in a new tab. They are only used by this tab, the profile is not changed.
    /// </summary>
    private async Task ScanProfileAs()
    {
        var profile = SelectedProfile;

        var (isOK, windowsCredentials) = await WindowsCredentialsChildWindow.ShowAsync(Application.Current.MainWindow,
            Strings.ScanAs, NETworkManager.Profiles.Application.IPScanner.GetWindowsCredentials(profile));

        if (!isOK)
            return;

        // SNMP version and credentials of the profile, otherwise of its group, otherwise the settings (null)
        var snmpCredentials = NETworkManager.Profiles.Application.IPScanner.GetSNMPCredentials(profile);

        // No source: the credentials were entered for the tab
        AddTab(profile.IPScanner_HostOrIPRange, true, snmpCredentials,
            profile.IPScanner_OverrideSNMP ? profile.Name : profile.Group, windowsCredentials);
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
    /// Selects a tab (e.g. the tab that sent a notification). Nothing happens if it was closed or moved to another
    /// window.
    /// </summary>
    /// <param name="tabId">Id of the tab.</param>
    public void SelectTab(Guid tabId)
    {
        for (var i = 0; i < TabItems.Count; i++)
        {
            if (TabItems[i].Id != tabId)
                continue;

            SelectedTabIndex = i;
            return;
        }
    }

    /// <summary>
    /// Adds a new tab for the specified host or IP range.
    /// </summary>
    /// <param name="hostOrIPRange">The host or IP range to scan.</param>
    /// <param name="start">Starts it on the first load. If false, the input is only filled in.</param>
    /// <param name="snmpCredentials">
    /// SNMP version and credentials of a profile / group, null for the ones of the settings.
    /// </param>
    /// <param name="snmpCredentialsSource">Name of the profile / group of <paramref name="snmpCredentials" />.</param>
    /// <param name="windowsCredentials">
    /// Windows credentials of a profile / group, null for the user running the application.
    /// </param>
    /// <param name="windowsCredentialsSource">Name of the profile / group of <paramref name="windowsCredentials" />.</param>
    public void AddTab(string hostOrIPRange = null, bool start = true, SNMPSecureCredentials snmpCredentials = null,
        string snmpCredentialsSource = null, WindowsCredentials windowsCredentials = null,
        string windowsCredentialsSource = null)
    {
        var tabId = Guid.NewGuid();

        TabItems.Add(new DragablzTabItem(Strings.NewTab,
            new IPScannerView(tabId, hostOrIPRange, start, snmpCredentials, snmpCredentialsSource, windowsCredentials,
                windowsCredentialsSource), tabId));

        SelectedTabIndex = TabItems.Count - 1;
    }

    /// <summary>
    /// Adds a new tab for the specified profile.
    /// </summary>
    /// <param name="profile">The profile to scan.</param>
    /// <param name="start">Starts it on the first load. If false, the input is only filled in.</param>
    public void AddTab(ProfileInfo profile, bool start = true)
    {
        // SNMP version and credentials of the profile, otherwise of its group, otherwise the settings (null)
        var snmpCredentials = NETworkManager.Profiles.Application.IPScanner.GetSNMPCredentials(profile);

        // Windows credentials of the profile, otherwise of its group, otherwise the user running the application (null)
        var windowsCredentials = NETworkManager.Profiles.Application.IPScanner.GetWindowsCredentials(profile);

        AddTab(profile.IPScanner_HostOrIPRange, start, snmpCredentials,
            profile.IPScanner_OverrideSNMP ? profile.Name : profile.Group, windowsCredentials,
            profile.IPScanner_UseWindowsCredentials ? profile.Name : profile.Group);
    }

    #endregion
}
