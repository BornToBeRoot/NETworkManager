using NETworkManager.ViewModels;
using System;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PingView : UserControl
    {
        PingViewModel viewModel;

        public PingView(int tabId, string host = null)
        {
            InitializeComponent();

            viewModel = new PingViewModel(tabId, host);

            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            viewModel.OnLoaded();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            viewModel.OnClose();
        }

        public void CloseTab()
        {
            viewModel.OnClose();
        }
    }
}
