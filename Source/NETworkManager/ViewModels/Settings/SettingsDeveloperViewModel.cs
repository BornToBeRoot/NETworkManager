using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsDeveloperViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private bool _developerMode;
        public bool DeveloperMode
        {
            get { return _developerMode; }
            set
            {
                if (value == _developerMode)
                    return;

                if (!_isLoading)
                {
                    SettingsManager.Current.DeveloperMode = value;

                    SettingsManager.RestartRequired = true;
                }

                _developerMode = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsDeveloperViewModel()
        {
            LoadSettings();

            _isLoading = true;
        }

        private void LoadSettings()
        {
            DeveloperMode = SettingsManager.Current.DeveloperMode;
        }
        #endregion
    }
}