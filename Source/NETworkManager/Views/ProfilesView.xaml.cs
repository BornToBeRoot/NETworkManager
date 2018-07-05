using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class ProfilesView : UserControl
    {
        private bool _isFirstLoad = true;
        private ProfilesViewModel viewModel = new ProfilesViewModel(DialogCoordinator.Instance);

        public ProfilesView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            viewModel.EditProfile();
        }

        public void Refresh()
        {
            viewModel.Refresh();
        }
    }
}
