using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupView
    {
        private readonly DNSLookupViewModel _viewModel;

        public DNSLookupView(int tabId, string host = null)
        {
            InitializeComponent();

            _viewModel = new DNSLookupViewModel(tabId, host);

            DataContext = _viewModel;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.OnLoaded();
        }

        public void CloseTab()
        {
            _viewModel.OnClose();
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu) menu.DataContext = _viewModel;
        }
    }
}
