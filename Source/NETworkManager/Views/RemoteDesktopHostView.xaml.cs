using NETworkManager.Utilities.WPF;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class RemoteDesktopHostView : IApplicationRequestHandler
{
    private readonly RemoteDesktopHostViewModel _viewModel = new();

    public RemoteDesktopHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public async void Open(ApplicationRequest request)
    {
        // Wait for the interface to load, before displaying the dialog to connect a new Profile...
        // MahApps will throw an exception...
        if (!await this.WaitForLoadedAsync())
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
