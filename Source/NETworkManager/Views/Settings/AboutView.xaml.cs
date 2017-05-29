using NETworkManager.ViewModels.Settings;
using System.Windows.Controls;

namespace NETworkManager.Views.Settings
{
    public partial class AboutView : UserControl
    {
        private AboutViewModel viewModel = new AboutViewModel();

        public AboutView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
