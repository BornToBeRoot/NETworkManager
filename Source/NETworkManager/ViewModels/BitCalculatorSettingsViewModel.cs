using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class BitCalculatorSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        /*
        private int _defaultPort;
        public int DefaultPort
        {
            get => _defaultPort;
            set
            {
                if (value == _defaultPort)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.WakeOnLAN_Port = value;

                _defaultPort = value;
                OnPropertyChanged();
            }
        }
        */
        #endregion

        #region Constructor, load settings
        public BitCalculatorSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
          //  DefaultPort = SettingsManager.Current.WakeOnLAN_Port;
        }
        #endregion
    }
}