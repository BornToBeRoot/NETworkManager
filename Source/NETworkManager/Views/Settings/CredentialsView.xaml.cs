using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Settings;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class CredentialsView : UserControl
    {
        private CredentialsViewModel viewModel = new CredentialsViewModel(DialogCoordinator.Instance);

        public CredentialsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.CheckCredentialsLoaded();
        }
    }
}
