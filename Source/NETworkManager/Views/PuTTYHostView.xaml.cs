using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PuTTYHostView
    {
        private readonly PuTTYHostViewModel _viewModel = new PuTTYHostViewModel(DialogCoordinator.Instance);

        private bool _loaded;


        public PuTTYHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationViewManager.Name.PuTTY.ToString();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _loaded = true;
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu) menu.DataContext = _viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                _viewModel.ConnectProfileCommand.Execute(null);
        }

        public async void AddTab(string host)
        {
            // Wait for the interface to load, before displaying the dialog to connect a new Profile... 
            // MahApps will throw an exception... 
            while (!_loaded)
                await Task.Delay(100);

            if (_viewModel.IsPuTTYConfigured)
                _viewModel.AddTab(host);
        }

        public void Refresh()
        {
            _viewModel.Refresh();
        }
    }
}
