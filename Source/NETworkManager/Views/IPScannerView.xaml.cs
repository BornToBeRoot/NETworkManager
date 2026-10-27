using System;
using System.Collections;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using NETworkManager.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;
using VisualTreeHelper = System.Windows.Media.VisualTreeHelper;

namespace NETworkManager.Views;

public partial class IPScannerView : IDragablzTabItem
{
    private readonly IPScannerViewModel _viewModel;

    public IPScannerView(Guid tabId, string hostOrIPRange = null, bool start = true)
    {
        InitializeComponent();

        _viewModel = new IPScannerViewModel(tabId, hostOrIPRange, start);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public void CloseTab()
    {
        // Detach the app-lifetime handler so this transient tab view (and its view model)
        // can be collected after the tab is closed.
        Dispatcher.ShutdownStarted -= Dispatcher_ShutdownStarted;

        _viewModel.OnClose();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        _viewModel.OnClose();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu menu)
            return;

        // Set DataContext to ViewModel
        menu.DataContext = _viewModel;

        // Append custom commands
        var index = menu.Items.Count - 1;

        var entryFound = false;

        for (var i = 0; i < menu.Items.Count; i++)
        {
            if (menu.Items[i] is not MenuItem item)
                continue;

            if ((string)item.Tag != "CustomCommands")
                continue;

            index = i;

            entryFound = true;

            break;
        }

        if (!entryFound)
            return;

        // Clear existing items in custom commands
        ((MenuItem)menu.Items[index])?.Items.Clear();

        // Add items to custom commands
        foreach (var info in IPScannerViewModel.CustomCommands)
            ((MenuItem)menu.Items[index])?.Items.Add(new MenuItem
            {
                Header = info.Name,
                Command = _viewModel.CustomCommandCommand,
                CommandParameter = info.ID
            });
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        // Get the row from the sender
        for (var visual = sender as Visual; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
        {
            if (visual is not DataGridRow row)
                continue;

            row.DetailsVisibility =
                row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            break;
        }
    }

    private void DataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        var column = e.Column;

        var selectedComparer = -1;

        switch (column.SortMemberPath)
        {
            case $"{nameof(PingInfo)}.{nameof(PingInfo.IPAddress)}":
                selectedComparer = 0;
                break;
            case nameof(IPScannerHostInfo.MACAddress):
                selectedComparer = 1;
                break;
            case $"{nameof(PingInfo)}.{nameof(PingInfo.Time)}":
                selectedComparer = 2;
                break;
            default:
                return;
        }

        // Prevent the built-in sort from sorting
        e.Handled = true;

        // Get the direction
        var direction = column.SortDirection != ListSortDirection.Ascending
            ? ListSortDirection.Ascending
            : ListSortDirection.Descending;

        // Update the sort direction (only one column is sorted - the built-in sort would reset the other columns)
        foreach (var dataGridColumn in ((DataGrid)sender).Columns)
            dataGridColumn.SortDirection = null;

        e.Column.SortDirection = direction;

        // Get the view
        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(((DataGrid)sender).ItemsSource);

        // Sort the view
        view.CustomSort = new DataGridComparer(direction, selectedComparer);
    }

    public class DataGridComparer(ListSortDirection direction, int comparer = -1) : IComparer
    {
        public int Compare(object x, object y)
        {
            // No comparer selected
            if (comparer == -1)
                return 0;

            // Get data from objects
            if (x is not IPScannerHostInfo first || y is not IPScannerHostInfo second)
                return 0;

            // Compare the data
            var result = comparer switch
            {
                // IP address
                0 => direction == ListSortDirection.Ascending
                    ? IPAddressHelper.CompareIPAddresses(first.PingInfo.IPAddress, second.PingInfo.IPAddress)
                    : IPAddressHelper.CompareIPAddresses(second.PingInfo.IPAddress, first.PingInfo.IPAddress),
                // MAC address - hosts without a MAC address always last
                1 => CompareMACAddressNoMACLast(first, second),
                // Ping time - hosts without a ping reply always last (their time is 0)
                2 => direction == ListSortDirection.Ascending
                    ? GetPingTime(first).CompareTo(GetPingTime(second))
                    : CompareDescendingNoReplyLast(first, second),
                _ => 0
            };

            // Same MAC address / time (e.g. all hosts without a MAC address or ping reply): by IP address - the sort is
            // not stable, the order of these hosts would be random otherwise
            return result != 0 || comparer == 0
                ? result
                : IPAddressHelper.CompareIPAddresses(first.PingInfo.IPAddress, second.PingInfo.IPAddress);
        }

        /// <summary>
        ///     Compares the MAC address in the sort direction, hosts without a MAC address (not resolved, routed host,
        ///     or still resolving) last. An empty MAC address can't be converted to bytes.
        /// </summary>
        private int CompareMACAddressNoMACLast(IPScannerHostInfo first, IPScannerHostInfo second)
        {
            var x = first.MACAddress;
            var y = second.MACAddress;

            if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
                return string.IsNullOrEmpty(x).CompareTo(string.IsNullOrEmpty(y));

            return direction == ListSortDirection.Ascending
                ? MACAddressHelper.CompareMACAddresses(x, y)
                : MACAddressHelper.CompareMACAddresses(y, x);
        }

        /// <summary>
        ///     Gets the ping time for sorting, <see cref="long.MaxValue" /> without a ping reply (or while the ping is
        ///     still running).
        /// </summary>
        private static long GetPingTime(IPScannerHostInfo info)
        {
            return !info.IsPingPending && info.PingInfo.Status == IPStatus.Success
                ? info.PingInfo.Time
                : long.MaxValue;
        }

        /// <summary>
        ///     Compares the ping time descending, hosts without a ping reply still last.
        /// </summary>
        private static int CompareDescendingNoReplyLast(IPScannerHostInfo first, IPScannerHostInfo second)
        {
            var x = GetPingTime(first);
            var y = GetPingTime(second);

            if (x == long.MaxValue || y == long.MaxValue)
                return x.CompareTo(y);

            return y.CompareTo(x);
        }
    }
}