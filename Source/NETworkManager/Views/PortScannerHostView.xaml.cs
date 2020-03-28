using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using NETworkManager.Settings;
using NETworkManager.Profiles;
using NETworkManager.Models;

namespace NETworkManager.Views
{
    public partial class PortScannerHostView
    {
        private readonly PortScannerHostViewModel _viewModel = new PortScannerHostViewModel(DialogCoordinator.Instance);

        public PortScannerHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationName.PortScanner.ToString();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                _viewModel.ScanProfileCommand.Execute(null);
        }

        public void AddTab(string host)
        {
            _viewModel.AddTab(host);
        }

        public void AddTab(ProfileInfo profile)
        {
            _viewModel.AddTab(profile);
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
