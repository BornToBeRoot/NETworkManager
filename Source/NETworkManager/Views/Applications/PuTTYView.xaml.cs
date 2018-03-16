using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class PuTTYView : UserControl
    {
        PuTTYViewModel viewModel = new PuTTYViewModel();

        public PuTTYView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
