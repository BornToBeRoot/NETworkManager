using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NETworkManager.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PortScannerView : IDragablzTabItem
{
    private readonly PortScannerViewModel _viewModel;

    public PortScannerView(Guid tabId, string host = null, string ports = null, bool start = true,
        string message = null)
    {
        InitializeComponent();

        _viewModel = new PortScannerViewModel(tabId, host, ports, start, message);

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

    private void DataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        // IPAddress doesn't implement IComparable - the built-in sort would throw as soon as
        // the results contain more than one host
        if (e.Column.SortMemberPath != nameof(PortScannerPortInfo.IPAddress))
            return;

        // Prevent the built-in sort from sorting
        e.Handled = true;

        // Get the direction
        var direction = e.Column.SortDirection != ListSortDirection.Ascending
            ? ListSortDirection.Ascending
            : ListSortDirection.Descending;

        // Update the sort direction (only one column is sorted)
        foreach (var column in ((DataGrid)sender).Columns)
            column.SortDirection = null;

        e.Column.SortDirection = direction;

        // Get the view
        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(((DataGrid)sender).ItemsSource);

        // Sort the view
        view.CustomSort = new DataGridComparer(direction);
    }

    /// <summary>
    ///     Sorts the results by IP address and port.
    /// </summary>
    public class DataGridComparer(ListSortDirection direction) : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is not PortScannerPortInfo first || y is not PortScannerPortInfo second)
                return 0;

            var result = IPAddressHelper.CompareIPAddresses(first.IPAddress, second.IPAddress);

            if (result == 0)
                result = first.Port.CompareTo(second.Port);

            return direction == ListSortDirection.Ascending ? result : -result;
        }
    }
}