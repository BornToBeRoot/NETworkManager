using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SpeedTestWidgetView
{
    private readonly SpeedTestWidgetViewModel _viewModel = new();

    public SpeedTestWidgetView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }
}
