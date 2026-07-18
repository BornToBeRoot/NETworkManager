using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WebConsoleHostView
{
    private readonly WebConsoleHostViewModel _viewModel = new();

    public WebConsoleHostView()
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
