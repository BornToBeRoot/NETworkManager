using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class RDAPHostView : IApplicationRequestHandler
{
    private readonly RDAPHostViewModel _viewModel = new();

    public RDAPHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
        if (request.Profile != null)
            _viewModel.AddTab(request.Profile, request.Start);
        else
            _viewModel.AddTab(request.Target, (request as RDAPRequest)?.QueryType, request.Start);
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
