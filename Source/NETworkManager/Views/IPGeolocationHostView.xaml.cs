using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPGeolocationHostView
{
    private readonly IPGeolocationHostViewModel _viewModel = new();

    public IPGeolocationHostView()
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
