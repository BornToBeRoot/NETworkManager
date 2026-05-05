using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NETworkManager.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DNSLookupView : IDragablzTabItem
{
    private readonly DNSLookupViewModel _viewModel;

    public DNSLookupView(Guid tabId, string host = null)
    {
        InitializeComponent();

        _viewModel = new DNSLookupViewModel(tabId, host);

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

    // Force star-sized DataGrid columns to recompute on first row load. Without this, an
    // initially empty DataGrid sizes star columns to MinWidth because the inner ScrollViewer
    // measures with infinite width; only a window resize triggers a correct re-measure.
    private void DataGridResults_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        DataGridResults.LoadingRow -= DataGridResults_LoadingRow;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (var column in DataGridResults.Columns)
            {
                if (!column.Width.IsStar)
                    continue;

                var width = column.Width;
                column.Width = 0;
                column.Width = width;
            }
        }), DispatcherPriority.ContextIdle);
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
}