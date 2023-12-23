using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class TracerouteSettingsView
{
    private readonly TracerouteSettingsViewModel _viewModel = new();

    public TracerouteSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}