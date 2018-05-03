using System;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupView : UserControl
    {
        DNSLookupViewModel viewModel;

        public DNSLookupView(int tabId)
        {
            InitializeComponent();

            viewModel = new DNSLookupViewModel(tabId);

            DataContext = viewModel;
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
