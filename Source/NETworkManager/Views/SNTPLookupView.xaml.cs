using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NETworkManager.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SNTPLookupView : IDragablzTabItem
{
    private readonly SNTPLookupViewModel _viewModel;

    public SNTPLookupView(Guid tabId)
    {
        InitializeComponent();

        _viewModel = new SNTPLookupViewModel(tabId);

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

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }
}