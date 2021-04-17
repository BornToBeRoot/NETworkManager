using System.Windows;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class NetworkConnectionView
    {
        private readonly NetworkConnectionViewModel _viewModel = new NetworkConnectionViewModel();

        public NetworkConnectionView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
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
}
