using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class CredentialsView : UserControl
    {
        private CredentialsViewModel viewModel = new CredentialsViewModel(DialogCoordinator.Instance);

        public CredentialsView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
