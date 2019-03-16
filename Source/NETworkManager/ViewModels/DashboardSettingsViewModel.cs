using NETworkManager.Models.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class DashboardSettingsViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;
        
        private bool _checkPublicIPAddress;
        public bool CheckPublicIPAddress
        {
            get => _checkPublicIPAddress;
            set
            {
                if (value == _checkPublicIPAddress)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Dashboard_CheckPublicIPAddress = value;

                _checkPublicIPAddress = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Contructor, load settings
        public DashboardSettingsViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            CheckPublicIPAddress = SettingsManager.Current.Dashboard_CheckPublicIPAddress;
        }
        #endregion
    }
}