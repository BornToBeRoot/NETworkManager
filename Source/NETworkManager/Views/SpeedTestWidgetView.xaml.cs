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

    /// <summary>
    ///     Cancels a running speed test (e.g. the widget was hidden in the settings).
    /// </summary>
    public void Cancel()
    {
        _viewModel.Cancel();
    }
}
