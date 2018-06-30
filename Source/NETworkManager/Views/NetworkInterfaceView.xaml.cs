using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class NetworkInterfaceView : UserControl
    {
        NetworkInterfaceViewModel viewModel = new NetworkInterfaceViewModel(DialogCoordinator.Instance);

        public NetworkInterfaceView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }

        public void Refresh()
        {
            viewModel.Refresh();
        }
    }
}
