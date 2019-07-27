using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class SettingsStatusViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;
     
        #endregion

        #region Contructor, load settings
        public SettingsStatusViewModel()
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