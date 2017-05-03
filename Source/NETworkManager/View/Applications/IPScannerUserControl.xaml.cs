using System;
using System.Windows.Controls;
using NETworkManager.ViewModel.Applications;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.View.Applications
{
    public partial class IPScannerUserControl : UserControl
    {
        IPScannerViewModel viewModel = new IPScannerViewModel(DialogCoordinator.Instance);

        public IPScannerUserControl()
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
