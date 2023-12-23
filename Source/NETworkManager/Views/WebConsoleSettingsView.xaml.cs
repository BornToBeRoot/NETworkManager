using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WebConsoleSettingsView
{
    private readonly WebConsoleSettingsViewModel _viewModel = new();

    public WebConsoleSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}