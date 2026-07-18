using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPScannerHostView
{
    private readonly IPScannerHostViewModel _viewModel = new();

    public IPScannerHostView()
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
