using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class HTTPHeadersHostView : UserControl
    {
        HTTPHeadersHostViewModel viewModel = new HTTPHeadersHostViewModel();

        public HTTPHeadersHostView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }               
    }
}
