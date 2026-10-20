using NETworkManager.ViewModels;

namespace NETworkManager.Views;

/// <summary>
/// View for the Firewall settings.
/// </summary>
public partial class FirewallSettingsView
{
    /// <summary>
    /// ViewModel of the view.
    /// </summary>
    private readonly FirewallSettingsViewModel _viewModel = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FirewallSettingsView"/> class.
    /// </summary>
    public FirewallSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}