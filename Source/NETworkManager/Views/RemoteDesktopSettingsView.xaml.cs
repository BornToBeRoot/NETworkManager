using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class RemoteDesktopSettingsView
{
    private readonly RemoteDesktopSettingsViewModel _viewModel = new();

    public RemoteDesktopSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}