using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class MDNSDiscoveryView : IApplicationRequestHandler
{
    private readonly MDNSDiscoveryViewModel _viewModel = new();

    public MDNSDiscoveryView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void OnViewVisible() => _viewModel.OnViewVisible();
    public void OnViewHide() => _viewModel.OnViewHide();
    public void Open(ApplicationRequest request) => _ = _viewModel.OpenAsync(request);
}
