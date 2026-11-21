using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class NetworkInterfaceView : IApplicationRequestHandler
{
    private readonly NetworkInterfaceViewModel _viewModel = new();

    public NetworkInterfaceView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }

    public void Open(ApplicationRequest request)
    {
        _viewModel.Open(request);
    }
}
