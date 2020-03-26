using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class WebConsoleHostView
    {
        private readonly WebConsoleHostViewModel _viewModel = new WebConsoleHostViewModel(DialogCoordinator.Instance);

        public WebConsoleHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = Models.Application.ApplicationName.WebConsole.ToString();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                _viewModel.ConnectProfileCommand.Execute(null);
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
