using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
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
