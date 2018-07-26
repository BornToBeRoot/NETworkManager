using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class ProfilesView
    {
        private readonly ProfilesViewModel _viewModel = new ProfilesViewModel(DialogCoordinator.Instance);

        public ProfilesView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu) menu.DataContext = _viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.EditProfile();
        }

        public void Refresh()
        {
            _viewModel.Refresh();
        }
    }
}
