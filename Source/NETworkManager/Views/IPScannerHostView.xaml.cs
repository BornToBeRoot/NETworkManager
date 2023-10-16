using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Models;

namespace NETworkManager.Views;

public partial class IPScannerHostView
{
    private readonly IPScannerHostViewModel _viewModel = new(DialogCoordinator.Instance);

    public IPScannerHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        InterTabController.Partition = ApplicationName.IPScanner.ToString();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            _viewModel.ScanProfileCommand.Execute(null);
    }

    public void AddTab(string host)
    {
        _viewModel.AddTab(host);
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
