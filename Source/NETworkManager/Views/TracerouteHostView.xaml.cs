using NETworkManager.Profiles;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class TracerouteHostView
{
    private readonly TracerouteHostViewModel _viewModel = new();

    public TracerouteHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void AddTab(string host)
    {
        _viewModel.AddTab(host);
    }

    public void AddTab(ProfileInfo profile)
    {
        _viewModel.AddTab(profile);
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
