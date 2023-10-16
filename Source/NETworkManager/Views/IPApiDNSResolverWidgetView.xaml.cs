using System.Windows;
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

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Check();
    }
}
