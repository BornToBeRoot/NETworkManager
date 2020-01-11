using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class WebConsoleView
    {
        private readonly WebConsoleViewModel _viewModel;

        public WebConsoleView(int tabId, string websiteUri = null)
        {
            InitializeComponent();

            _viewModel = new WebConsoleViewModel(DialogCoordinator.Instance, tabId, websiteUri);

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
