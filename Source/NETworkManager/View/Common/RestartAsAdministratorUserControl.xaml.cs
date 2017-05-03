using NETworkManager.ViewModel.Common;
using System.Windows.Controls;

namespace NETworkManager.View.Common
{
    public partial class RestartAsAdministratorUserControl : UserControl
    {
        RestartAsAdministratorViewModel viewModel = new RestartAsAdministratorViewModel();

        public RestartAsAdministratorUserControl()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
