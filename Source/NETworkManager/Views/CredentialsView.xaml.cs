using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views
{
    public partial class CredentialsView
    {
        private readonly CredentialsViewModel _viewModel = new CredentialsViewModel(DialogCoordinator.Instance);

        public CredentialsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu) menu.DataContext = _viewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.CheckCredentialsLoaded();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                _viewModel.Edit();
        }
    }
}
