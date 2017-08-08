using System.Windows.Controls;
using NETworkManager.ViewModels.Applications;
using MahApps.Metro.Controls.Dialogs;

namespace NETworkManager.Views.Applications
{
    public partial class DNSLookupView : UserControl
    {
        DNSLookupViewModel viewModel = new DNSLookupViewModel(DialogCoordinator.Instance);

        public DNSLookupView()
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
