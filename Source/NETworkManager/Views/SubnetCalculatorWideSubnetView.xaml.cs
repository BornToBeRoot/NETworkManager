using System;
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

    public void Calculate(string subnet1, string subnet2)
    {
        _viewModel.Calculate(subnet1, subnet2);
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        _viewModel.OnShutdown();
    }
}