using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class HTTPHeadersHostView 
    {
        private readonly HTTPHeadersHostViewModel _viewModel = new HTTPHeadersHostViewModel(DialogCoordinator.Instance);

        public HTTPHeadersHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = Models.Application.ApplicationName.HTTPHeaders.ToString();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                _viewModel.CheckProfileCommand.Execute(null);
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
