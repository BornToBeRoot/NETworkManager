using NETworkManager.ViewModels.Applications;
using System.Windows.Controls;

namespace NETworkManager.Views.Applications
{
    public partial class PingHostView : UserControl
    {
        PingHostViewModel viewModel = new PingHostViewModel();

        public PingHostView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
