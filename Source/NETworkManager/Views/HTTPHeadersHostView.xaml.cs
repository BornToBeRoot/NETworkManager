using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class HTTPHeadersHostView 
    {
        private readonly HTTPHeadersHostViewModel _viewModel = new HTTPHeadersHostViewModel();

        public HTTPHeadersHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationViewManager.Name.HTTPHeaders.ToString();
        }
    }
}
