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
            SubnettingView.Calculate(request.Target, subnetCalculatorRequest.NewSubnetmask, request.Start);
        }
        else if (subnetCalculatorRequest?.Subnet2 != null)
        {
            TabItemWideSubnet.IsSelected = true;
            WideSubnetView.Calculate(request.Target, subnetCalculatorRequest.Subnet2, request.Start);
        }
        else
        {
            TabItemCalculator.IsSelected = true;
            CalculatorView.Calculate(request.Target, request.Start);
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