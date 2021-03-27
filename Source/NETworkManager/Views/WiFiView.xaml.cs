using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class WiFiView
    {
        private readonly WiFiViewModel _viewModel; 

        public WiFiView()
        {
            _viewModel= new WiFiViewModel(DialogCoordinator.Instance);
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

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }
    }
}
