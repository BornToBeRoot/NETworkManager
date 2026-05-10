using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WiFiView
{
    private readonly WiFiViewModel _viewModel;

    public WiFiView()
    {
        _viewModel = new WiFiViewModel();

        InitializeComponent();

        DataContext = _viewModel;
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    // Force star-sized DataGrid columns to recompute on first row load. Without this, an
    // initially empty DataGrid sizes star columns to MinWidth because the inner ScrollViewer
    // measures with infinite width; only a window resize triggers a correct re-measure.
    private void DataGridNetworks_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        DataGridNetworks.LoadingRow -= DataGridNetworks_LoadingRow;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (var column in DataGridNetworks.Columns)
            {
                if (!column.Width.IsStar)
                    continue;

                var width = column.Width;
                column.Width = 0;
                column.Width = width;
            }
        }), DispatcherPriority.ContextIdle);
    }
}