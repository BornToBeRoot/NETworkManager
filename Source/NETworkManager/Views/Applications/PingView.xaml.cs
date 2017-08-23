using NETworkManager.ViewModels.Applications;
using System;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class PingView : UserControl
    {
        PingViewModel viewModel = new PingViewModel();
            
        public PingView()
        {
            InitializeComponent();
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            viewModel.OnShutdown();
        }
    }
}
