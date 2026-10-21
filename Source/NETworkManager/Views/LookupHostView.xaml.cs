using NETworkManager.Models.Lookup;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class LookupHostView : IApplicationRequestHandler
{
    private readonly LookupHostViewModel _viewModel = new();

    public LookupHostView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public void Open(ApplicationRequest request)
    {
        if (request is not LookupRequest lookupRequest)
            return;

        switch (lookupRequest.Type)
        {
            case LookupType.OUI:
                TabItemOUILookup.IsSelected = true;

                if (!string.IsNullOrEmpty(request.Target))
                    OUILookupView.Lookup(request.Target);

                break;
            case LookupType.Port:
                TabItemPortLookup.IsSelected = true;

                if (!string.IsNullOrEmpty(request.Target))
                    PortLookupView.Lookup(request.Target);

                break;
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