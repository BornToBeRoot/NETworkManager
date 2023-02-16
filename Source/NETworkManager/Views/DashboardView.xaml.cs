using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DashboardView
    {
        private readonly DashboardViewModel _viewModel = new();

        private NetworkConnectionView _networkConnectiondView;

        public DashboardView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            _networkConnectiondView = new NetworkConnectionView();
            ContentControlNetworkConnection.Content = _networkConnectiondView;
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
