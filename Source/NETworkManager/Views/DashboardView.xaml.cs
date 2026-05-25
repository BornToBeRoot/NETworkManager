using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    private readonly NetworkConnectionWidgetView _networkConnectionWidgetView = new();
    private readonly IPApiIPGeolocationWidgetView _ipApiIPGeolocationWidgetView = new();
    private readonly IPApiDNSResolverWidgetView _ipApiDNSResolverWidgetView = new();
    private readonly SpeedTestWidgetView _speedTestWidgetView = new();


    public DashboardView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // Load views
        ContentControlNetworkConnection.Content = _networkConnectionWidgetView;
        ContentControlIPApiIPGeolocation.Content = _ipApiIPGeolocationWidgetView;
        ContentControlIPApiDNSResolver.Content = _ipApiDNSResolverWidgetView;
        ContentControlSpeedTest.Content = _speedTestWidgetView;

        // Check all widgets
        _networkConnectionWidgetView.Check();
        _ipApiIPGeolocationWidgetView.Check();
        _ipApiDNSResolverWidgetView.Check();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();

        // Check network connection
        _networkConnectionWidgetView.Check();
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }
}
