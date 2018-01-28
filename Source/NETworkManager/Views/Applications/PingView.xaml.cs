using NETworkManager.ViewModels.Applications;
using System;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class PingView : UserControl
    {
        PingViewModel viewModel;
            
        public PingView(int tabId, Action<Tuple<int, string>> changeTabTitle)
        {
            InitializeComponent();

            viewModel = new PingViewModel(tabId, changeTabTitle);

            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            viewModel.OnShutdown();
        }
    }
}
