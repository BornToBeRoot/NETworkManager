using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class LLDPCDPCaptureView
{
    private readonly LLDPCDPCaptureViewModel _viewModel = new();

    public LLDPCDPCaptureView()
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