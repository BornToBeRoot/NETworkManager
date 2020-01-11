using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels
{
    public class WebConsoleSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;
        #endregion

        #region Contructor, load settings
        public WebConsoleSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
        }
        #endregion
    }
}