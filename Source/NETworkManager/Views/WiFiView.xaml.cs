using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        _viewModel.Networks.CollectionChanged += Networks_CollectionChanged;
    }

    /// <summary>
    ///     Scrolls the selected network into view after the networks were replaced by a scan (it is selected again,
    ///     see <see cref="WiFiViewModel.SelectedNetwork" />). Not when the user changes the selection (e.g. Ctrl+click),
    ///     the grid would jump.
    /// </summary>
    private void Networks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Reset)
            return;

        // Wait until the network is selected again and the DataGrid has processed the new items and the selection
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            var network = _viewModel.SelectedNetwork;

            // The network may be hidden by the filter / search
            if (network != null && DataGridNetworks.Items.Contains(network))
                DataGridNetworks.ScrollIntoView(network);
        }));
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }

    /// <summary>
    ///     Scrolls the list of the channel charts with the mouse wheel, also over a chart or the legend of a chart (an
    ///     own horizontal ScrollViewer, it would handle the mouse wheel).
    /// </summary>
    private void ScrollViewerChannels_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;

        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);

        e.Handled = true;
    }
}