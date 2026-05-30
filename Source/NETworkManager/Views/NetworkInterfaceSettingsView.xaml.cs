using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class NetworkInterfaceSettingsView
{
    private readonly NetworkInterfaceSettingsViewModel _viewModel = new();

    public NetworkInterfaceSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}
