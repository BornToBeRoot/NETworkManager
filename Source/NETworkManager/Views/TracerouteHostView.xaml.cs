using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class TracerouteHostView : UserControl
    {
        TracerouteHostViewModel viewModel = new TracerouteHostViewModel(DialogCoordinator.Instance);

        public TracerouteHostView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                viewModel.TraceProfileCommand.Execute(null);
        }

        public void AddTab(string host)
        {
            viewModel.AddTab(host);
        }
    }
}
