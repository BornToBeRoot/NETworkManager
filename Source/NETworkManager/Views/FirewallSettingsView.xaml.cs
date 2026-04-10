using NETworkManager.ViewModels;

namespace NETworkManager.Views;

/// <summary>
/// View for the firewall settings.
/// </summary>
public partial class FirewallSettingsView
{
    private readonly FirewallSettingsViewModel _viewModel = new();
    
    public FirewallSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}