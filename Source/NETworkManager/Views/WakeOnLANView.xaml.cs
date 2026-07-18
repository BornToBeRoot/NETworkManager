using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WakeOnLANView
{
    private readonly WakeOnLANViewModel _viewModel = new();

    public WakeOnLANView()
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
