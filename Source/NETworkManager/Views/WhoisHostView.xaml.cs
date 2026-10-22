using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WhoisHostView : IApplicationRequestHandler
{
    private readonly WhoisHostViewModel _viewModel = new();

    public WhoisHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
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
