using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class DNSLookupHostView
{
    private readonly DNSLookupHostViewModel _viewModel = new();

    public DNSLookupHostView()
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
