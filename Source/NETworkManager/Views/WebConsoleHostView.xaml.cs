using NETworkManager.Utilities.WPF;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class WebConsoleHostView : IApplicationRequestHandler
{
    private readonly WebConsoleHostViewModel _viewModel = new();

    public WebConsoleHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public async void Open(ApplicationRequest request)
    {
        // Wait for the interface to load, before displaying the dialog to connect a new profile...
        // MahApps will throw an exception...
        if (!await this.WaitForLoadedAsync())
            return;

        // WebView2 runtime is required for the web console.
        if (!_viewModel.IsRuntimeAvailable)
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
