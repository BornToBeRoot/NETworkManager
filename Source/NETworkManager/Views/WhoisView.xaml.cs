using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class WhoisView
    {
        private readonly WhoisViewModel _viewModel;

        public WhoisView(int tabId)
        {
            InitializeComponent();

            _viewModel = new WhoisViewModel(tabId);

            DataContext = _viewModel;
        }

        public void CloseTab()
        {
            _viewModel.OnClose();
        }
    }
}
