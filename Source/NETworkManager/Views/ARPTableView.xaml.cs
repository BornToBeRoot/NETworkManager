using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Views;

public partial class ARPTableView
{
    private readonly ARPTableViewModel _viewModel = new(DialogCoordinator.Instance);

    public ARPTableView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
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
            case nameof(ARPInfo.IPAddress):
                selectedComparer = 0;
                break;
            case nameof(ARPInfo.MACAddress):
                selectedComparer = 1;
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
            if (x is not ARPInfo first || y is not ARPInfo second)
                return 0;

            // Compare the data
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
                _ => 0
            };
        }
    }
}
