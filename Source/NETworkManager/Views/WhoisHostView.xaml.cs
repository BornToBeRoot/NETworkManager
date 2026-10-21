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
            _viewModel.AddTab(request.Profile);
        else
            _viewModel.AddTab(request.Target);
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
