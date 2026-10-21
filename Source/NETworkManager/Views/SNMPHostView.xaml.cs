using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SNMPHostView : IApplicationRequestHandler
{
    private readonly SNMPHostViewModel _viewModel = new();

    public SNMPHostView()
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
