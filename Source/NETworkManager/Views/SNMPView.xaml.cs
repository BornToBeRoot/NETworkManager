using NETworkManager.ViewModels;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.Network;

namespace NETworkManager.Views;

public partial class SNMPView
{
    private readonly SNMPViewModel _viewModel;

    public SNMPView(int tabId, SNMPSessionInfo sessionInfo)
    {
        InitializeComponent();

        _viewModel = new SNMPViewModel(DialogCoordinator.Instance, tabId, sessionInfo);

        DataContext = _viewModel;
    }

    public void CloseTab()
    {
        _viewModel.OnClose();
    }

    private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }
}
