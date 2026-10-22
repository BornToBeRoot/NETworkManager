using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WakeOnLANView : IApplicationRequestHandler
{
    private readonly WakeOnLANViewModel _viewModel = new();

    public WakeOnLANView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
        if (request.Profile != null)
            _viewModel.WakeUp(request.Profile, request.Start);
        else
            _viewModel.WakeUp(request.Target, (request as WakeOnLANRequest)?.Broadcast, request.Start);
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
