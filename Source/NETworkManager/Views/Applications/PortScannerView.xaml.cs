using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
      public partial class PortScannerView : UserControl
    {
        PortScannerViewModel viewModel = new PortScannerViewModel(DialogCoordinator.Instance);

        public PortScannerView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
