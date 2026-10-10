using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class RDAPHostView
{
    private readonly RDAPHostViewModel _viewModel = new();

    public RDAPHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void AddTab(string query)
    {
        _viewModel.AddTab(query);
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
