using NETworkManager.ViewModels;

namespace NETworkManager.Views;

/// <summary>
/// View for the firewall settings.
/// </summary>
public partial class FirewallSettingsView
{
    /// <summary>
    /// DataContext for the view.
    /// </summary>
    private readonly FirewallSettingsViewModel _viewModel = 
            FirewallSettingsViewModel.Instance;

    /// <summary>
    /// Construct the view.
    /// </summary>
    public FirewallSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}