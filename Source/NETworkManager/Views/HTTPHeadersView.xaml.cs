using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class HTTPHeadersView
    {
        private readonly HTTPHeadersViewModel _viewModel;

        public HTTPHeadersView(int tabId)
        {
            InitializeComponent();

            _viewModel = new HTTPHeadersViewModel(tabId);

            DataContext = _viewModel;
        }

        public void CloseTab()
        {
            _viewModel.OnClose();
        }
    }
}
