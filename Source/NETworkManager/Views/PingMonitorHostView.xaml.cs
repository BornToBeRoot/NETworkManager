using System.Windows;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PingMonitorHostView
{
    private readonly PingMonitorHostViewModel _viewModel = new();

    public PingMonitorHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void AddHost(string host)
    {
        if (_viewModel.SetHost(host))
            _ = _viewModel.Start();
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
    ///     Opens the group actions context menu on a normal click (not just right-click), so it
    ///     works the same way for mouse, touch and keyboard activation. PlacementTarget is set
    ///     explicitly since it isn't populated automatically when opening the menu this way, and
    ///     the menu items rely on it to reach the group name (see Button.Tag in the XAML).
    /// </summary>
    private void GroupActionsButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.IsOpen = true;
    }
}
