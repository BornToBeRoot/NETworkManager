using NETworkManager.ViewModels;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views
{
    public partial class ListenersView
    {
        private readonly ListenersViewModel _viewModel = new ListenersViewModel(DialogCoordinator.Instance);

        public ListenersView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
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
