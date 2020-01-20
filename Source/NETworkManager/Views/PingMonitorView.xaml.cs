using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PingMonitorView
    {
        private readonly PingMonitorViewModel _viewModel = new PingMonitorViewModel(DialogCoordinator.Instance);

        public PingMonitorView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
      
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                _viewModel.AddHostProfileCommand.Execute(null);
        }

        public void AddHost(string host)
        {
            _viewModel.AddHost(host);
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
