using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class ListenersView
{
    private readonly ListenersViewModel _viewModel = new(DialogCoordinator.Instance);

    public ListenersView()
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

        if (column.SortMemberPath != nameof(ListenerInfo.IPAddress))
            return;

        // Prevent the built-in sort from sorting
        e.Handled = true;

        // Get the direction
        var direction = column.SortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        // Update the sort direction
        column.SortDirection = direction;

        // Get the view
        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(((DataGrid)sender).ItemsSource);

        // Sort the view
        view.CustomSort = new DataGridComparer(direction);
    }

    public class DataGridComparer(ListSortDirection direction) : IComparer
    {
        public int Compare(object x, object y)
        {
            // Get data from objects
            if (x is not ListenerInfo first || y is not ListenerInfo second)
                return 0;

            // Compare the data
            return direction == ListSortDirection.Ascending
                ? IPAddressHelper.CompareIPAddresses(first.IPAddress, second.IPAddress)
                : IPAddressHelper.CompareIPAddresses(second.IPAddress, first.IPAddress);
        }
    }
}