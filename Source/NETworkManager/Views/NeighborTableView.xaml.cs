using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class NeighborTableView
{
    private readonly NeighborTableViewModel _viewModel = new();

    public NeighborTableView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }

    private void DataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        var column = e.Column;

        var selectedComparer = -1;

        switch (column.SortMemberPath)
        {
            case nameof(NeighborInfo.IPAddress):
                selectedComparer = 0;
                break;
            case nameof(NeighborInfo.MACAddress):
                selectedComparer = 1;
                break;
            case nameof(NeighborInfo.InterfaceAlias):
                selectedComparer = 2;
                break;
            case nameof(NeighborInfo.State):
                selectedComparer = 3;
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

        // Update the sort direction
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
            if (x is not NeighborInfo first || y is not NeighborInfo second)
                return 0;

            return comparer switch
            {
                // IP address
                0 => direction == ListSortDirection.Ascending
                    ? IPAddressHelper.CompareIPAddresses(first.IPAddress, second.IPAddress)
                    : IPAddressHelper.CompareIPAddresses(second.IPAddress, first.IPAddress),
                // MAC address
                1 => direction == ListSortDirection.Ascending
                    ? MACAddressHelper.CompareMACAddresses(first.MACAddress, second.MACAddress)
                    : MACAddressHelper.CompareMACAddresses(second.MACAddress, first.MACAddress),
                // Interface alias
                2 => direction == ListSortDirection.Ascending
                    ? string.Compare(first.InterfaceAlias, second.InterfaceAlias, System.StringComparison.OrdinalIgnoreCase)
                    : string.Compare(second.InterfaceAlias, first.InterfaceAlias, System.StringComparison.OrdinalIgnoreCase),
                // State
                3 => direction == ListSortDirection.Ascending
                    ? ((int)first.State).CompareTo((int)second.State)
                    : ((int)second.State).CompareTo((int)first.State),
                _ => 0
            };
        }
    }
}
