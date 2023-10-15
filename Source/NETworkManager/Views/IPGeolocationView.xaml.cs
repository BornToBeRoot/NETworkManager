using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class IPGeolocationView
{
    private readonly IPGeolocationViewModel _viewModel;

    public IPGeolocationView(int tabId, string domain = null)
    {
        InitializeComponent();

        _viewModel = new IPGeolocationViewModel(DialogCoordinator.Instance, tabId, domain);

        DataContext = _viewModel;
    }

    private void UserControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

    public void CloseTab()
    {
        _viewModel.OnClose();
    }
}
