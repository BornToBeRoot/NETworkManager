using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    private NetworkConnectionView _networkConnectiondView;
    private IPApiIPGeolocationView _ipApiIPGeolocationView;
    private IPApiDNSResolverView _ipApiDNSResolverView;

    public DashboardView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // Network connection
        _networkConnectiondView = new NetworkConnectionView();
        ContentControlNetworkConnection.Content = _networkConnectiondView;

        // IP Geolocation Api
        _ipApiIPGeolocationView = new IPApiIPGeolocationView();
        ContentControlIPApiIPGeolocation.Content = _ipApiIPGeolocationView;

        // IP DNS Api
        _ipApiDNSResolverView = new IPApiDNSResolverView();
        ContentControlIPApiDNSResolver.Content = _ipApiDNSResolverView; 
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
