using NETworkManager.Utilities.WPF;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class TigerVNCHostView : IApplicationRequestHandler
{
    private readonly TigerVNCHostViewModel _viewModel = new();

    public TigerVNCHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public async void Open(ApplicationRequest request)
    {
        // Wait for the interface to load, before displaying the dialog to connect a new profile...
        // MahApps will throw an exception...
        await this.WaitForLoadedAsync();

        if (!_viewModel.IsConfigured)
            return;

        if (request.Profile != null)
            _viewModel.ConnectProfile(request.Profile);
        else
            _viewModel.AddTab(request.Target);
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
