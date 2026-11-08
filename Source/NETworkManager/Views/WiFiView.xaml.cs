using System;
using System.ComponentModel;
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

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    /// <summary>
    ///     Scrolls the selected network into view, e.g. when it is selected again after the networks were scanned
    ///     again (see <see cref="WiFiViewModel.SelectedNetwork" />).
    /// </summary>
    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(WiFiViewModel.SelectedNetwork) || _viewModel.SelectedNetwork == null)
            return;

        // Wait until the DataGrid has processed the new items and the selection
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