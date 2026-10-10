using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WhoisHostView
{
    private readonly WhoisHostViewModel _viewModel = new();

    public WhoisHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void AddTab(string domain)
    {
        _viewModel.AddTab(domain);
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
