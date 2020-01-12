using NETworkManager.Models.Settings;

namespace NETworkManager.ViewModels
{
    public class WebConsoleSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private bool _ignoreCertificateErrors;
        public bool IgnoreCertificateErrors
        {
            get => _ignoreCertificateErrors;
            set
            {
                if (value == _ignoreCertificateErrors)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.WebConsole_IgnoreCertificateErrors = value;

                _ignoreCertificateErrors = value;
                OnPropertyChanged();
            }
        }
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
            IgnoreCertificateErrors = SettingsManager.Current.WebConsole_IgnoreCertificateErrors;
        }
        #endregion
    }
}