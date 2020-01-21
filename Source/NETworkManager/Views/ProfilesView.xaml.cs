using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                _viewModel.EditProfile();
        }

        public void Refresh()
        {
            _viewModel.RefreshProfiles();
        }


    }
}
