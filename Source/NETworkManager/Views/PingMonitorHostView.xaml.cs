using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PingMonitorHostView
{
    private readonly PingMonitorHostViewModel _viewModel = new();

    public PingMonitorHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void AddHost(string host)
    {
        if (_viewModel.SetHost(host))
            _ = _viewModel.Start();
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
