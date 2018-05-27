using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class LookupHostView : UserControl
    {
        LookupHostViewModel viewModel = new LookupHostViewModel();

        public LookupHostView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
