using System.Windows;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DashboardView
    {
        private readonly DashboardViewModel _viewModel = new DashboardViewModel();

        public DashboardView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        public void OnViewHide()
        {
            _viewModel.OnViewHide();
        }

        public void OnViewVisible()
        {
            _viewModel.OnViewVisible();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }
    }
}
