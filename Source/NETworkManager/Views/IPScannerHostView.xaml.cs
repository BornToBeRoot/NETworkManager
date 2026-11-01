using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPScannerHostView : IApplicationRequestHandler
{
    private readonly IPScannerHostViewModel _viewModel = new();

    public IPScannerHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
        // E.g. a click on a notification of the tab
        if (request is ShowTabRequest showTabRequest)
        {
            _viewModel.SelectTab(showTabRequest.TabId);
            return;
        }

        if (request.Profile != null)
            _viewModel.AddTab(request.Profile, request.Start);
        else
            _viewModel.AddTab(request.Target, start: request.Start);
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
