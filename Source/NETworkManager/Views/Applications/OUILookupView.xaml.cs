using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class OUILookupView : UserControl
    {
        OUILookupViewModel viewModel = new OUILookupViewModel(DialogCoordinator.Instance);

        public OUILookupView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
