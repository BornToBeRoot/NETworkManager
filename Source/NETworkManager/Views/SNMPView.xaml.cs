using NETworkManager.ViewModels;
using System;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class SNMPView : UserControl
    {
        SNMPViewModel viewModel;

        public SNMPView(int tabId, string host = null)
        {
            InitializeComponent();

            viewModel = new SNMPViewModel(tabId, host);

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
