using NETworkManager.Models.Settings;
using NETworkManager.Utils;

namespace NETworkManager.ViewModels
{
    public class SettingsUpdateViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private bool _checkForUpdatesAtStartup;
        public bool CheckForUpdatesAtStartup
        {
            get { return _checkForUpdatesAtStartup; }
            set
            {
                if (value == _checkForUpdatesAtStartup)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Update_CheckForUpdatesAtStartup = value;

                _checkForUpdatesAtStartup = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsUpdateViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            CheckForUpdatesAtStartup = SettingsManager.Current.Update_CheckForUpdatesAtStartup;
        }
        #endregion
    }
}