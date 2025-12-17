using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class BitCalculatorView
{
    private readonly BitCalculatorViewModel _viewModel = new();

    public BitCalculatorView()
    {
        InitializeComponent();
        DataContext = _viewModel;
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