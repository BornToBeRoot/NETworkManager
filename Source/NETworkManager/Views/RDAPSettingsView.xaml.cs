using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class RDAPSettingsView
{
    private readonly RDAPSettingsViewModel _viewModel = new();

    public RDAPSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}
