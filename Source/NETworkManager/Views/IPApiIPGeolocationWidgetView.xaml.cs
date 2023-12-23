using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPApiIPGeolocationWidgetView
{
    private readonly IPApiIPGeolocationWidgetViewModel _viewModel = new();

    public IPApiIPGeolocationWidgetView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Check();
    }
}