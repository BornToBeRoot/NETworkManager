using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPApiDNSResolverWidgetView
{
    private readonly IPApiDNSResolverWidgetViewModel _viewModel = new();

    public IPApiDNSResolverWidgetView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Check()
    {
        _viewModel.Check();
    }
}