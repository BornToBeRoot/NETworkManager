using NETworkManager.Settings;

namespace NETworkManager.ViewModels
{
    public class WhoisSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        private bool _useRipe;
        public bool UseRipe
        {
            get => _useRipe;
            set
            {
                if (value == _useRipe)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Whois_UseRipe= value;

                _useRipe = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load settings
        public WhoisSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            UseRipe = SettingsManager.Current.Whois_UseRipe;
        }
        #endregion
    }
}
