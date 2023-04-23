using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models;
using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views;

public partial class SNMPHostView
{
    private readonly SNMPHostViewModel _viewModel = new(DialogCoordinator.Instance);

    public SNMPHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        InterTabController.Partition = ApplicationName.SNMP.ToString();
    }

    private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            _viewModel.AddTabProfileCommand.Execute(null);
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
