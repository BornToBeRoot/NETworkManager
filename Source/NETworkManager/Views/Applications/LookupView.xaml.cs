using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class LookupView : UserControl
    {
        LookupViewModel viewModel = new LookupViewModel(DialogCoordinator.Instance);

        public LookupView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
