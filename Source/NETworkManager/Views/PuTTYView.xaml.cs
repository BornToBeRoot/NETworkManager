using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows.Controls;

namespace NETworkManager.Views
{
    public partial class PuTTYView : UserControl
    {
        PuTTYViewModel viewModel = new PuTTYViewModel(DialogCoordinator.Instance);

        public PuTTYView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            menu.DataContext = viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                viewModel.ConnectSessionCommand.Execute(null);
        }
    }
}
