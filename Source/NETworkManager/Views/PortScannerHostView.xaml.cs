using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PortScannerHostView : IApplicationRequestHandler
{
    private readonly PortScannerHostViewModel _viewModel = new();

    public PortScannerHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
        if (request.Profile != null)
            _viewModel.AddTab(request.Profile, request.Start);
        else
            _viewModel.AddTab(request.Target, (request as PortScannerRequest)?.Ports, request.Start);
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
