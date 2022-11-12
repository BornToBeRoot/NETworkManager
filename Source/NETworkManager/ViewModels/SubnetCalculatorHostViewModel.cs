namespace NETworkManager.ViewModels
{
    public class SubnetCalculatorHostViewModel : ViewModelBase
    {
        private bool _isViewActive = true;

        public SubnetCalculatorHostViewModel()
        {

        }

        public void OnViewVisible()
        {
            _isViewActive = true;
        }

        public void OnViewHide()
        {
            _isViewActive = false;
        }
    }
}
