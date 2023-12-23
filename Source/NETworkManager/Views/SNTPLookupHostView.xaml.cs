using NETworkManager.Models;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class SNTPLookupHostView
{
    private readonly SNTPLookupHostViewModel _viewModel = new();

    public SNTPLookupHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;

        InterTabController.Partition = ApplicationName.SNTPLookup.ToString();
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