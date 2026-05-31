using System.Windows;
using System.Windows.Controls;
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

    /// <summary>
    ///     Restores the previously selected channel chart tab. The content is rebuilt when switching
    ///     between the outer tabs, so the selection is restored from the view model on load.
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
        if (sender is TabControl tabControl && tabControl.IsLoaded &&
            ReferenceEquals(e.OriginalSource, tabControl))
            _channelTabControlSelectedIndex = tabControl.SelectedIndex;
    }
}