using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    private NetworkConnectionView _networkConnectiondView;
    private IPGeoApiView _ipGeoApiView;

    public DashboardView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // Network connection
        _networkConnectiondView = new NetworkConnectionView();
        ContentControlNetworkConnection.Content = _networkConnectiondView;

        // IP Geolocation Api
        _ipGeoApiView = new IPGeoApiView();
        ContentControlIPGeoApi.Content = _ipGeoApiView;
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
