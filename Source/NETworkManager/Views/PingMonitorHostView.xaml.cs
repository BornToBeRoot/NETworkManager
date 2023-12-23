using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PingMonitorHostView
{
    private readonly PingMonitorHostViewModel _viewModel = new(DialogCoordinator.Instance);

    public PingMonitorHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            _viewModel.PingProfileCommand.Execute(null);
    }

    public void AddHost(string host)
    {
        if (_viewModel.SetHost(host))
            _viewModel.Start().ConfigureAwait(false);
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }
}