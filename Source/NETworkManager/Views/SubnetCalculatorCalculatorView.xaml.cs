using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SubnetCalculatorCalculatorView
{
    private readonly SubnetCalculatorCalculatorViewModel _viewModel = new();

    public SubnetCalculatorCalculatorView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Calculate(string subnet, bool start)
    {
        _viewModel.Calculate(subnet, start);
    }
}