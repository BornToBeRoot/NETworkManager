using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DashboardView
{
    private readonly DashboardViewModel _viewModel = new();

    private readonly NetworkConnectionWidgetView _networkConnectionWidgetView = new();
    private readonly IPApiIPGeolocationWidgetView _ipApiIPGeolocationWidgetView = new();
    private readonly IPApiDNSResolverWidgetView _ipApiDNSResolverWidgetView = new();
    

    public DashboardView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // Load views
        ContentControlNetworkConnection.Content = _networkConnectionWidgetView;
        ContentControlIPApiIPGeolocation.Content = _ipApiIPGeolocationWidgetView;
        ContentControlIPApiDNSResolver.Content = _ipApiDNSResolverWidgetView;
        
        // Check all widgets
        Check();
    }
    
    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
        
        // Check all widgets
        Check();
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }
    
    /// <summary>
    /// Check all widgets
    /// </summary>
    private void Check()
    {
        _networkConnectionWidgetView.Check();
        _ipApiIPGeolocationWidgetView.Check();
        _ipApiDNSResolverWidgetView.Check();
    }
}