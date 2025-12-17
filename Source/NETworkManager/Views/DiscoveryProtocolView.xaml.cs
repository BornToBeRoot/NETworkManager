using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DiscoveryProtocolView
{
    private readonly DiscoveryProtocolViewModel _viewModel = new();

    public DiscoveryProtocolView()
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
}