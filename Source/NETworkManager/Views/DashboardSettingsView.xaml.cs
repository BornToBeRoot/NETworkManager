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
}