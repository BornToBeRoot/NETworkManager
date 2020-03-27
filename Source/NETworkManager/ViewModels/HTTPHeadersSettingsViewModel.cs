using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class HTTPHeadersSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;
        
        private int _timeout;
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value == _timeout)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.HTTPHeaders_Timeout = value;

                _timeout = value;
                OnPropertyChanged();
            }
        }

        private bool _showStatistics;
        public bool ShowStatistics
        {
            get => _showStatistics;
            set
            {
                if (value == _showStatistics)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.HTTPHeaders_ShowStatistics = value;

                _showStatistics = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public HTTPHeadersSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            Timeout = SettingsManager.Current.HTTPHeaders_Timeout;
            ShowStatistics = SettingsManager.Current.HTTPHeaders_ShowStatistics;
        }
        #endregion
    }
}