using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PingMonitorSettingsView
{
    private readonly PingMonitorSettingsViewModel _viewModel = new();

    public PingMonitorSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}
