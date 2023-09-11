using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SubnetCalculatorWideSubnetView
{
    private readonly SubnetCalculatorWideSubnetViewModel _viewModel = new();

    public SubnetCalculatorWideSubnetView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void Dispatcher_ShutdownStarted(object sender, System.EventArgs e)
    {
        _viewModel.OnShutdown();
    }
}
