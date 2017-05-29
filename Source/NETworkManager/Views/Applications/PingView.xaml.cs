using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;
using System;

namespace NETworkManager.Views.Applications
{
    public partial class PingView : UserControl
    {
        PingViewModel viewModel = new PingViewModel(DialogCoordinator.Instance);
            
        public PingView()
        {
            InitializeComponent();
            DataContext = viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
        {
            viewModel.OnShutdown();
        }
    }
}
