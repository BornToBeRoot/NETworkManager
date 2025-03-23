using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class HostsFileEditorView
{
    private readonly HostsFileEditorViewModel _viewModel = new(DialogCoordinator.Instance);

    public HostsFileEditorView()
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
