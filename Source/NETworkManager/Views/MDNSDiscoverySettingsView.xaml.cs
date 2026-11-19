using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class MDNSDiscoverySettingsView
{
    private readonly MDNSDiscoverySettingsViewModel _viewModel = new();

    public MDNSDiscoverySettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.EditServiceTypeProfileCommand.CanExecute(null))
            _viewModel.EditServiceTypeProfileCommand.Execute(null);
    }
}