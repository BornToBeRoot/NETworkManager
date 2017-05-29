using NETworkManager.ViewModels.Notes;
using System.Windows.Controls;

namespace NETworkManager.Views.Notes
{
    public partial class RestartAsAdministratorView : UserControl
    {
        RestartAsAdministratorViewModel viewModel = new RestartAsAdministratorViewModel();

        public RestartAsAdministratorView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
