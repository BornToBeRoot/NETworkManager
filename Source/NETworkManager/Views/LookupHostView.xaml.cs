using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class LookupHostView
    {
        private readonly LookupHostViewModel _viewModel = new LookupHostViewModel();

        public LookupHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
