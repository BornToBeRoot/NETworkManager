using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views;

public partial class DNSLookupSettingsView
{
    private readonly DNSLookupSettingsViewModel _viewModel = new();

    public DNSLookupSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.EditDNSServerCommand.CanExecute(null))
            _viewModel.EditDNSServerCommand.Execute(null);
    }
}