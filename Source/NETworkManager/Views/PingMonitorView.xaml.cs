using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PingMonitorView
    {
        private readonly PingMonitorViewModel _viewModel = new PingMonitorViewModel(DialogCoordinator.Instance);

        public PingMonitorView()
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
