using NETworkManager.ViewModels;

namespace NETworkManager.Views;

/// <summary>
/// View for the WiFi settings.
/// </summary>
public partial class WiFiSettingsView
{
    /// <summary>
    /// ViewModel of the view.
    /// </summary>
    private readonly WiFiSettingsViewModel _viewModel = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WiFiSettingsView"/> class.
    /// </summary>
    public WiFiSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}
