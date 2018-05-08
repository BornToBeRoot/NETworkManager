using System;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupView : UserControl
    {
        DNSLookupViewModel viewModel;

        public DNSLookupView(int tabId, string host = null)
        {
            InitializeComponent();

            viewModel = new DNSLookupViewModel(tabId, host);

            DataContext = viewModel;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.OnLoaded();
        }

        public void CloseTab()
        {
            viewModel.OnClose();
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }
    }
}
