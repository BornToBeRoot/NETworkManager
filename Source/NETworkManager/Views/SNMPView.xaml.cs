using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SNMPView : IDragablzTabItem
{
    private readonly SNMPViewModel _viewModel;

    public SNMPView(Guid tabId, SNMPSessionInfo sessionInfo)
    {
        InitializeComponent();

        _viewModel = new SNMPViewModel(DialogCoordinator.Instance, tabId, sessionInfo);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public void CloseTab()
    {
        _viewModel.OnClose();
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        _viewModel.OnClose();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        var column = e.Column;

        if (column.SortMemberPath != nameof(SNMPInfo.OID))
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
            if (x is not SNMPInfo first || y is not SNMPInfo second)
                return 0;

            // Compare the data
            return direction == ListSortDirection.Ascending
                ? SNMPOIDHelper.CompareOIDs(first.OID, second.OID)
                : SNMPOIDHelper.CompareOIDs(second.OID, first.OID);
        }
    }
}