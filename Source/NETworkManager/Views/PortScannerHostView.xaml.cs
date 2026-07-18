using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PortScannerHostView
{
    private readonly PortScannerHostViewModel _viewModel = new();

    public PortScannerHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void AddTab(string host)
    {
        _viewModel.AddTab(host);
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
