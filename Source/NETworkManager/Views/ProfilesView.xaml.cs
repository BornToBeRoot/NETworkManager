using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class ProfilesView
{
    private readonly ProfilesViewModel _viewModel = new(DialogCoordinator.Instance);

    public ProfilesView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }

    private void DataGridGroupsRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            if (_viewModel.EditGroupCommand.CanExecute(null))
                _viewModel.EditGroupCommand.Execute(null);
    }

    private void DataGridProfilesRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            if (_viewModel.EditProfileCommand.CanExecute(null))
                _viewModel.EditProfileCommand.Execute(null);
    }

    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }
}