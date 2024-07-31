using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class AWSSessionManagerHostView
{
    private readonly AWSSessionManagerHostViewModel _viewModel = new(DialogCoordinator.Instance);

    private bool _loaded;

    public AWSSessionManagerHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
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

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            _viewModel.ConnectProfileCommand.Execute(null);
    }

    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    public void OnViewVisible(bool fromSettings)
    {
        _viewModel.OnViewVisible(fromSettings);
    }

    public void OnProfileLoaded()
    {
        _viewModel.OnProfileLoaded();
    }

    public void FocusEmbeddedWindow()
    {
        _viewModel.FocusEmbeddedWindow();
    }
}