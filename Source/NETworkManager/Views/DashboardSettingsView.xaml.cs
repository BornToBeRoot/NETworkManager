using System.Windows.Input;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardSettingsView
{
    private readonly DashboardSettingsViewModel _viewModel = new();

    public DashboardSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.EditLatencyTargetCommand.CanExecute(null))
            _viewModel.EditLatencyTargetCommand.Execute(null);
    }
}