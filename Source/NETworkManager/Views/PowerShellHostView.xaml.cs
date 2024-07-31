using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class PowerShellHostView
{
    private readonly PowerShellHostViewModel _viewModel = new(DialogCoordinator.Instance);

    private bool _loaded;

    public PowerShellHostView()
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

    public async void AddTab(string host)
    {
        // Wait for the interface to load, before displaying the dialog to connect a new profile... 
        // MahApps will throw an exception... 
        while (!_loaded)
            await Task.Delay(250);

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

    public void FocusEmbeddedWindow()
    {
        _viewModel.FocusEmbeddedWindow();
    }
}