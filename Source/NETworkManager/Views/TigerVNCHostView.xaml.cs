using NETworkManager.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace NETworkManager.Views;

public partial class TigerVNCHostView
{
    private readonly TigerVNCHostViewModel _viewModel = new();

    private bool _loaded;

    public TigerVNCHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;
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
}
