using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DNSLookupHostView : IApplicationRequestHandler
{
    private readonly DNSLookupHostViewModel _viewModel = new();

    public DNSLookupHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
        if (request.Profile != null)
        {
            _viewModel.AddTab(request.Profile);
            return;
        }

        var dnsLookupRequest = request as DNSLookupRequest;

        _viewModel.AddTab(request.Target, dnsLookupRequest?.DNSServer, dnsLookupRequest?.QueryType);
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
