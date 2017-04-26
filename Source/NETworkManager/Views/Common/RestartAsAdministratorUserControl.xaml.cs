using NETworkManager.ViewModels.Common;
using System.Windows.Controls;

namespace NETworkManager.Views.Common
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
