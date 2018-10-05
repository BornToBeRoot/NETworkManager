using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class WhoisHostView 
    {
        private readonly WhoisHostViewModel _viewModel = new WhoisHostViewModel();

        public WhoisHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationViewManager.Name.HTTPHeaders.ToString();
        }
    }
}
