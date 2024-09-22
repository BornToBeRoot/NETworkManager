using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPApiIPGeolocationWidgetView
{
    private readonly IPApiIPGeolocationWidgetViewModel _viewModel = new();

    public IPApiIPGeolocationWidgetView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Check()
    {
        _viewModel.Check();
    }
}