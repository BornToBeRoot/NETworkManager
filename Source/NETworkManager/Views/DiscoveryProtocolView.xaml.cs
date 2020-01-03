using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DiscoveryProtocolView
    {
        private readonly DiscoveryProtocolViewModel _viewModel = new DiscoveryProtocolViewModel(DialogCoordinator.Instance);

        public DiscoveryProtocolView()
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
