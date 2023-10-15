using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views;

public partial class SNMPSettingsView
{
    private readonly SNMPSettingsViewModel _viewModel = new(DialogCoordinator.Instance);

    public SNMPSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void RowContextMenu_OnOpened(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        _viewModel.EditOIDProfile().ConfigureAwait(false);
    }
}
