using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class SettingsUpdateViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private bool _checkForUpdatesAtStartup;
        public bool CheckForUpdatesAtStartup
        {
            get => _checkForUpdatesAtStartup;
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

        private bool _checkForPreReleases;
        public bool CheckForPreReleases
        {
            get => _checkForPreReleases;
            set
            {
                if (value == _checkForPreReleases)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Update_CheckForPreReleases = value;

                _checkForPreReleases = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsUpdateViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            CheckForUpdatesAtStartup = SettingsManager.Current.Update_CheckForUpdatesAtStartup;
            CheckForPreReleases = SettingsManager.Current.Update_CheckForPreReleases;
        }
        #endregion
    }
}