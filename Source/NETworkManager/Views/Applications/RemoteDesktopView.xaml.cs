using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class RemoteDesktopView : UserControl
    {
        RemoteDesktopViewModel viewModel = new RemoteDesktopViewModel(DialogCoordinator.Instance);

        public RemoteDesktopView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
