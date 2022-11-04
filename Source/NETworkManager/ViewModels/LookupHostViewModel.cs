namespace NETworkManager.ViewModels
{
    public class LookupHostViewModel : ViewModelBase
    {
        private bool _isViewActive = true;

        public LookupHostViewModel()
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
