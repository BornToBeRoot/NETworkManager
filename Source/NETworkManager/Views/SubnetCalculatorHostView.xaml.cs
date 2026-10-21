using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SubnetCalculatorHostView : IApplicationRequestHandler
{
    private readonly SubnetCalculatorHostViewModel _viewModel = new();

    public SubnetCalculatorHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
        var subnetCalculatorRequest = request as SubnetCalculatorRequest;

        if (subnetCalculatorRequest?.NewSubnetmask != null)
        {
            TabItemSubnetting.IsSelected = true;
            SubnettingView.Calculate(request.Target, subnetCalculatorRequest.NewSubnetmask);
        }
        else if (subnetCalculatorRequest?.Subnet2 != null)
        {
            TabItemWideSubnet.IsSelected = true;
            WideSubnetView.Calculate(request.Target, subnetCalculatorRequest.Subnet2);
        }
        else
        {
            TabItemCalculator.IsSelected = true;
            CalculatorView.Calculate(request.Target);
        }
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