namespace NETworkManager.ViewModels;

public class SubnetCalculatorHostViewModel : ViewModelBase
{
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool _isViewActive = true;
#pragma warning restore CS0414 // Field is assigned but its value is never used

    public void OnViewVisible()
    {
        _isViewActive = true;
    }

    public void OnViewHide()
    {
        _isViewActive = false;
    }
}
