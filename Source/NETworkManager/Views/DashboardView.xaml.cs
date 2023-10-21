using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    public DashboardView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // Load views
        ContentControlNetworkConnection.Content = new NetworkConnectionWidgetView();
        ContentControlIPApiIPGeolocation.Content =  new IPApiIPGeolocationWidgetView();
        ContentControlIPApiDNSResolver.Content =  new IPApiDNSResolverWidgetView();
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }
}
