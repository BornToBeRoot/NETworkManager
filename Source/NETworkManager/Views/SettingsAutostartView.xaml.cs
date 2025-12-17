using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SettingsAutostartView
{
    private readonly SettingsAutostartViewModel _viewModel = new();

    public SettingsAutostartView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}