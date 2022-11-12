using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class LookupHostView
    {
        private readonly LookupHostViewModel _viewModel = new();

        public LookupHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        public void OnViewHide()
        {
            _viewModel.OnViewHide();
        }

        public void OnViewVisible()
        {
            _viewModel.OnViewVisible();
        }
    }
}
