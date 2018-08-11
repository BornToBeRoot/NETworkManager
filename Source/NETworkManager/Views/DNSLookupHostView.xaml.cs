using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupHostView
    {
        private readonly DNSLookupHostViewModel _viewModel = new DNSLookupHostViewModel();

        public DNSLookupHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationViewManager.Name.DNSLookup.ToString();
        }

        public void AddTab(string host)
        {
            _viewModel.AddTab(host);
        }
    }
}
