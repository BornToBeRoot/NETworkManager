using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsApplicationWakeOnLANViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private int _defaultPort;
        public int DefaultPort
        {
            get { return _defaultPort; }
            set
            {
                if (value == _defaultPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.WakeOnLAN_DefaultPort = value;

                _defaultPort = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public SettingsApplicationWakeOnLANViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            DefaultPort = SettingsManager.Current.WakeOnLAN_DefaultPort;
        }
        #endregion
    }
}