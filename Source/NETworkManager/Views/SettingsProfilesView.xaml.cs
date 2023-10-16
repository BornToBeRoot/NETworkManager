using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Views;

public partial class SettingsProfilesView
{
    private readonly SettingsProfilesViewModel _viewModel = new(DialogCoordinator.Instance);

    public SettingsProfilesView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CloseAction != null)
            return;

        var window = Window.GetWindow(this);

        if (window != null)
            _viewModel.CloseAction = window.Close;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _viewModel.EditProfileFileCommand.Execute(null);
    }
}
