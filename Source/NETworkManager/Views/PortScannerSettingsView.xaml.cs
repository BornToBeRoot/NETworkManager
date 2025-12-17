using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PortScannerSettingsView
{
    private readonly PortScannerSettingsViewModel _viewModel = new();

    public PortScannerSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void RowContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.EditPortProfileCommand.CanExecute(null))
            _viewModel.EditPortProfileCommand.Execute(null);
    }
}