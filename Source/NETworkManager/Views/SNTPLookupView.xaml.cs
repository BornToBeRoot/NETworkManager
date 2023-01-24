using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class SNTPLookupView
    {
        private readonly SNTPLookupViewModel _viewModel;

        public SNTPLookupView(int tabId, string host = null)
        {
            InitializeComponent();

            _viewModel = new SNTPLookupViewModel(DialogCoordinator.Instance, tabId, host);

            DataContext = _viewModel;
        }
        
        public void CloseTab()
        {
            _viewModel.OnClose();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
                menu.DataContext = _viewModel;
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridColumnHeader columnHeader)
                _viewModel.SortResultByPropertyName(columnHeader.Column.SortMemberPath);
        }
    }
}
