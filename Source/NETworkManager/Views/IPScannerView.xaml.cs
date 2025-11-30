using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;
using VisualTreeHelper = System.Windows.Media.VisualTreeHelper;

namespace NETworkManager.Views;

public partial class IPScannerView : IDragablzTabItem
{
    private readonly IPScannerViewModel _viewModel;

    public IPScannerView(Guid tabId, string hostOrIPRange = null)
    {
        InitializeComponent();

        _viewModel = new IPScannerViewModel(DialogCoordinator.Instance, tabId, hostOrIPRange);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    public void CloseTab()
    {
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
            if (x is not IPScannerHostInfo first || y is not IPScannerHostInfo second)
                return 0;

            // Compare the data
            return comparer switch
            {
                // IP address
                0 => direction == ListSortDirection.Ascending
                    ? IPAddressHelper.CompareIPAddresses(first.PingInfo.IPAddress, second.PingInfo.IPAddress)
                    : IPAddressHelper.CompareIPAddresses(second.PingInfo.IPAddress, first.PingInfo.IPAddress),
                // MAC address
                1 => direction == ListSortDirection.Ascending
                    ? MACAddressHelper.CompareMACAddresses(first.MACAddress, second.MACAddress)
                    : MACAddressHelper.CompareMACAddresses(second.MACAddress, first.MACAddress),
                _ => 0
            };
        }
    }
}