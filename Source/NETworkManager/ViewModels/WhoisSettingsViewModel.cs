using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class WhoisSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;
        
       
        #endregion

        #region Contructor, load settings
        public WhoisSettingsViewModel()
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