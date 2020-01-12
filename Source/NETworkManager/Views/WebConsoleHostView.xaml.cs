using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class WebConsoleHostView
    {
        private readonly WebConsoleHostViewModel _viewModel = new WebConsoleHostViewModel(DialogCoordinator.Instance);

        private bool _loaded;


        public WebConsoleHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = ApplicationViewManager.Name.WebConsole.ToString();
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
