using System;
using System.Windows.Controls;
using NETworkManager.ViewModels.Applications;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views.Applications
{
    public partial class IPScannerView : UserControl
    {
        IPScannerViewModel viewModel = new IPScannerViewModel(DialogCoordinator.Instance);

        public IPScannerView()
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
