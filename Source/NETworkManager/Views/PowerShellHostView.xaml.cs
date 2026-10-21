using NETworkManager.Utilities.WPF;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PowerShellHostView : IApplicationRequestHandler
{
    private readonly PowerShellHostViewModel _viewModel = new();

    public PowerShellHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public async void Open(ApplicationRequest request)
    {
        // Wait for the interface to load, before displaying the dialog to connect a new profile...
        // MahApps will throw an exception...
        await this.WaitForLoadedAsync();

        if (!_viewModel.IsExecutableConfigured)
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

    public void FocusEmbeddedWindow()
    {
        _viewModel.FocusEmbeddedWindow();
    }
}
