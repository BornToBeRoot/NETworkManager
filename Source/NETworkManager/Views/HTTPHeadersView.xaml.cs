using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class HTTPHeadersView
    {
        private readonly HTTPHeadersViewModel _viewModel;

        public HTTPHeadersView(int tabId, string websiteUri = null)
        {
            InitializeComponent();

            _viewModel = new HTTPHeadersViewModel(DialogCoordinator.Instance, tabId, websiteUri);

            DataContext = _viewModel;
        }

        private void UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.OnLoaded();
        }

        public void CloseTab()
        {
            _viewModel.OnClose();
        }
    }
}
