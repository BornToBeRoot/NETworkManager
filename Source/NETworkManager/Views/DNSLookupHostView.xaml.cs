using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class DNSLookupHostView
    {
        private readonly DNSLookupHostViewModel _viewModel = new DNSLookupHostViewModel(DialogCoordinator.Instance);

        public DNSLookupHostView()
        {
            InitializeComponent();
            DataContext = _viewModel;

            InterTabController.Partition = Models.Application.Name.DNSLookup.ToString();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                _viewModel.LookupProfileCommand.Execute(null);
        }

        public void AddTab(string host)
        {
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
