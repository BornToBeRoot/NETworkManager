using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SNMPHostView
    {
        private readonly SNMPHostViewModel _viewModel = new SNMPHostViewModel();

        public SNMPHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationViewManager.Name.PuTTY.ToString();
        }

        public void AddTab(string host)
        {
            _viewModel.AddTab(host);
        }
    }
}
