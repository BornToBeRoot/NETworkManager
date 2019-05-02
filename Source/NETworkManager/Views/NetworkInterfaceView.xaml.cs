using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class NetworkInterfaceView
    {
        private readonly NetworkInterfaceViewModel _viewModel = new NetworkInterfaceViewModel(DialogCoordinator.Instance);

        public NetworkInterfaceView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
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
}
