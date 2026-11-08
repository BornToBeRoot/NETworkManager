using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WiFiView
{
    private readonly WiFiViewModel _viewModel;

    private int _channelTabControlSelectedIndex = 0;

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
    ///     Restores the previously selected channel chart tab. The content is rebuilt when switching
    ///     between the outer tabs, so the selection is restored from the view when the control loads.
    /// </summary>
    private void ChannelsTabControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TabControl tabControl)
            tabControl.SelectedIndex = _channelTabControlSelectedIndex;
    }

    /// <summary>
    ///     Persists the selected channel chart tab. Ignores selection changes that occur while the
    ///     control is not yet loaded (e.g. during the initial template apply) so the saved value is
    ///     not overwritten with the default, and ignores bubbled events from nested selectors.
    /// </summary>
    private void ChannelsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is TabControl { IsLoaded: true } tabControl &&
            ReferenceEquals(e.OriginalSource, tabControl))
            _channelTabControlSelectedIndex = tabControl.SelectedIndex;
    }
}