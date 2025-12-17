using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SNTPLookupSettingsView
{
    private readonly SNTPLookupSettingsViewModel _viewModel = new();

    public SNTPLookupSettingsView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.EditServerCommand.CanExecute(null))
            _viewModel.EditServerCommand.Execute(null);
    }
}