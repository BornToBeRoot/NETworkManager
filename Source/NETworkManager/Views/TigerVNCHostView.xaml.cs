using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models;
using NETworkManager.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class TigerVNCHostView
    {
        private readonly TigerVNCHostViewModel _viewModel = new TigerVNCHostViewModel(DialogCoordinator.Instance);

        private bool _loaded;


        public TigerVNCHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationName.TigerVNC.ToString();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                _viewModel.ConnectProfileCommand.Execute(null);
        }

        public async void AddTab(string host)
        {
            // Wait for the interface to load, before displaying the dialog to connect a new profile... 
            // MahApps will throw an exception... 
            while (!_loaded)
                await Task.Delay(100);

            if (_viewModel.IsConfigured)
                _viewModel.AddTab(host);
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
