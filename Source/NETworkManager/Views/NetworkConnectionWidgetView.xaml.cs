using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class NetworkConnectionWidgetView
{
    private readonly NetworkConnectionWidgetViewModel _viewModel = new();

    public NetworkConnectionWidgetView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Reload()
    {
        _viewModel.CheckConnection();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.CheckConnection();
    }
}
